from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from pydantic import ValidationError

from frontend import (
    Pillar,
    PillarStation,
)
from pillar_station import StatusMessage as StatusMessagePillarStation, ChangeForm
from dron_station import (
    StatusMessage as StatusMessageDronStation,
    RegisterForm,
    AllStruct,
    DronStateChange,
)
from base_models import Coordinates
from base_inputs import EnterData, EnterForm

from managers import Manager
from db_work import (
    get_all_pillars,
    get_all_pillar_stations,
    get_id_dron_station_by_pillar,
)
from datetime import datetime
import asyncpg
import os
import uuid

app = FastAPI(
    version="0.2.0",
)

db_str_connection = os.getenv("DB_URL")
if db_str_connection is None:
    db_str_connection = "postgresql://drone_admin:12345678@localhost:5432/base"
manager = Manager()


@app.websocket("/pillar_station")
async def websocket_endpoint_pillar_station(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    pool = await asyncpg.create_pool(dsn=db_str_connection)
    await websocket.accept()
    try:
        data = EnterData(**await websocket.receive_json())
        if data.event != "enter":
            await websocket.send_json(
                StatusMessagePillarStation(
                    status="Err", message="First message must be enter"
                ).model_dump()
            )
            await websocket.close(code=1003)
            return

        ent = EnterForm(**data.data)
        client_id = await manager.pillar_station_enter(websocket, ent.id, pool)
        if client_id is None:
            return

        while True:
            data = EnterData(**await websocket.receive_json())
            match data.event:
                case "change_lamp_state":
                    ch = ChangeForm(**data.data)
                    await manager.change_pillar(uuid.UUID(ch.id), ch.status, pool)
                case _:
                    pass

    except WebSocketDisconnect:
        print(f"Клиент столб станция/{client_id} отключился")
    except ValidationError as exc:
        await manager._send_status_pillar_station(
            websocket, status="Err", message="You lost fields"
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await pool.close()


@app.websocket("/dron_station")
async def websocket_endpoint_dron_station(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    pool = await asyncpg.create_pool(dsn=db_str_connection)
    await websocket.accept()
    try:
        data = EnterData(**await websocket.receive_json())
        if data.event != "enter" and data.event != "register":
            await websocket.send_json(
                StatusMessageDronStation(
                    status="Err", message="First message must be enter"
                ).model_dump()
            )
            await websocket.close(code=1003)
            return
        match data.event:
            case "enter":
                ent = EnterForm(**data.data)
                client_id = await manager.dron_station_enter(websocket, ent.id, pool)
            case "register":
                reg = RegisterForm(**data.data)
                client_id = await manager.dron_station_register(websocket, reg, pool)
        if client_id is None:
            return
        await manager.get_drones(client_id, pool)
        await manager.get_pillars(client_id, pool)

        while True:
            data = EnterData(**await websocket.receive_json())
            match data.event:
                case "change_dron_state":
                    d_s = DronStateChange(**data.data)
                    await manager.change_dron_state(
                        client_id,
                        uuid.UUID(d_s.id),
                        d_s.status,
                        d_s.last_coordinates,
                        pool,
                    )
                case _:
                    pass

    except WebSocketDisconnect:
        print(f"Клиент дрон станция/{client_id} отключился")
    except ValidationError as exc:
        await manager._send_status_dron_station(
            websocket, status="Err", message="You lost fields"
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await pool.close()


@app.websocket("/frontend")
async def websocket_endpoint_frontend(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    pool = await asyncpg.create_pool(dsn=db_str_connection)
    await websocket.accept()
    try:
        await manager.send_first_message_frontend(websocket, pool)
        while True:
            data = EnterData(**await websocket.receive_json())
            match data.event:
                case "change_lamp_state":
                    ch = ChangeForm(**data.data)
                    await manager.change_pillar(uuid.UUID(ch.id), ch.status, pool)
                case _:
                    pass

    except WebSocketDisconnect:
        print(f"Клиент дрон станция/{client_id} отключился")
    except ValidationError as exc:
        await manager._send_status_frontend(
            websocket, status="Err", message="You lost fields"
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await pool.close()


@app.get("/all-pillars")
async def all_pillars_get():
    pool = await asyncpg.create_pool(dsn=db_str_connection)
    async with pool.acquire() as conn:
        return AllStruct(
            pillars=[
                Pillar(
                    id=str(p.id),
                    coordinates=Coordinates(x=int(p.latitude), y=int(p.longitude)),
                    state=p.state,
                    pillar_station_id=str(p.id_pillar_station),
                    last_update=datetime.now().isoformat(),
                    id_dron_station=None
                    if (await get_id_dron_station_by_pillar(conn, p.id)) is None
                    else str(await get_id_dron_station_by_pillar(conn, p.id)),
                )
                for p in await get_all_pillars(conn)
            ],
            pillar_stations=[
                PillarStation(
                    id=str(p_s.id),
                    coordinates=Coordinates(x=int(p_s.latitude), y=int(p_s.longitude)),
                    is_alive=p_s.is_alive,
                )
                for p_s in await get_all_pillar_stations(conn)
            ],
        ).model_dump()
