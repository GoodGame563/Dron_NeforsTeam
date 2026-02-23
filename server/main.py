from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from pydantic import ValidationError
from models import (
    StatusMessage,
    RegisterMessage,
    EnterMessage,
    GreetingMessage,
    BaseMessage,
    Pillar,
    PillarStation,
    Dron,
    DronStation,
    Coordinates,
)
from managers import StationManager
from db_work import (
    get_all_pillars,
    get_all_pillar_stations,
    get_all_dron_stations,
    get_drons_by_station,
)
from datetime import datetime
import asyncpg
import uuid

app = FastAPI(
    version="0.2.0",
)

db_str_connection = "postgresql://drone_admin:12345678@localhost:5432/base"
manager = StationManager()


@app.websocket("/pillar_station")
async def websocket_endpoint_pillar_station(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    await websocket.accept()
    try:
        data = await websocket.receive_json()
        type_event = data.get("event")
        if type_event != "enter":
            await websocket.send_json(
                StatusMessage(
                    status="Err", message="First message must be enter"
                ).model_dump()
            )
            await websocket.close(code=1003)
            return

        ent = EnterMessage(**data)
        client_id = await manager.pillar_station_enter(websocket, ent.client_id)
        if client_id is None:
            return

        while True:
            data = await websocket.receive_json()
            event = data.get("event")
            match event:
                case "lamp_off":
                    await manager.pillar_lamp_off(client_id, str(data.get("id_pillar")))
                case _:
                    pass

    except WebSocketDisconnect:
        print(f"Клиент столб станция/{client_id} отключился")
    except ValidationError as exc:
        await websocket.send_json(
            StatusMessage(status="Err", message="You lost fields").model_dump()
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await manager.remove_connection(client_id)


@app.websocket("/dron_station")
async def websocket_endpoint_dron_station(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    await websocket.accept()
    try:
        data = await websocket.receive_json()
        type_event = data.get("event")
        if type_event != "register" and type_event != "enter":
            await websocket.send_json(
                StatusMessage(
                    status="Err", message="First message must be register or enter"
                ).model_dump()
            )
            await websocket.close(code=1003)
            return
        if type_event == "enter":
            ent = EnterMessage(**data)
            client_id = await manager.dron_station_enter(websocket, ent.client_id)
        else:
            reg = RegisterMessage(**data)
            client_id = await manager.dron_station_register(websocket, reg)
        if client_id is None:
            return

        while True:
            data = await websocket.receive_json()
            event = data.get("event")
            match event:
                case "register_drons":
                    await manager.register_drons(client_id)
                case "get_drons":
                    await manager.get_drones(client_id)
                case "get_pillars":
                    await manager.get_pillars(client_id)
                case _:
                    pass

    except WebSocketDisconnect:
        print(f"Клиент дрон станция/{client_id} отключился")
    except ValidationError as exc:
        await websocket.send_json(
            StatusMessage(status="Err", message="You lost fields").model_dump()
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await manager.remove_connection(client_id)


@app.websocket("/frontend")
async def websocket_endpoint_frontend(websocket: WebSocket):
    client_id: uuid.UUID | None = None
    pool = await asyncpg.create_pool(dsn=db_str_connection)
    await websocket.accept()
    try:
        async with pool.acquire() as conn:
            await websocket.send_json(
                BaseMessage(
                    event="all_data",
                    data=GreetingMessage(
                        pillars=[
                            Pillar(
                                id=str(p.id),
                                coordinates=Coordinates(
                                    x=int(p.latitude), y=int(p.longitude)
                                ),
                                state=p.state,
                                pillar_station_id=str(p.id_pillar_station),
                                last_update=datetime.now().isoformat(),
                            )
                            for p in await get_all_pillars(conn)
                        ],
                        pillar_stations=[
                            PillarStation(
                                id=str(p_s.id),
                                coordinates=Coordinates(
                                    x=int(p_s.latitude), y=int(p_s.longitude)
                                ),
                                is_alive=p_s.is_alive,
                            )
                            for p_s in await get_all_pillar_stations(conn)
                        ],
                        dron_stations=[
                            DronStation(
                                id=str(d_s.id),
                                coordinates=Coordinates(
                                    x=int(d_s.latitude), y=int(d_s.longitude)
                                ),
                                radius=d_s.radius,
                                total_drone_count=d_s.total_drone_count,
                                total_lamps_count=d_s.total_lamps_count,
                                drons=[
                                    Dron(
                                        id=str(d.id),
                                        status=d.status,
                                        last_coordinates=(
                                            None
                                            if d.last_latitude is None
                                            or d.last_longitude is None
                                            else Coordinates(
                                                x=int(d.last_latitude),
                                                y=int(d.last_longitude),
                                            )
                                        ),
                                    )
                                    for d in await get_drons_by_station(
                                        conn=conn, id_dron_station=d_s.id
                                    )
                                ],
                            )
                            for d_s in await get_all_dron_stations(conn)
                        ],
                    ),
                ).model_dump()
            )

        # data = await websocket.receive_json()
        # type_event = data.get("event")
        # if type_event != "register" and type_event != "enter":
        #     await websocket.send_json(
        #         StatusMessage(
        #             status="Err", message="First message must be register or enter"
        #         ).model_dump()
        #     )
        #     await websocket.close(code=1003)
        #     return
        # if type_event == "enter":
        #     ent = EnterMessage(**data)
        #     client_id = await manager.dron_station_enter(websocket, ent.client_id)
        # else:
        #     reg = RegisterMessage(**data)
        #     client_id = await manager.dron_station_register(websocket, reg)
        # if client_id is None:
        #     return

        # while True:
        #     data = await websocket.receive_json()
        #     event = data.get("event")
        #     match event:
        #         case "register_drons":
        #             await manager.register_drons(client_id)
        #         case "get_drons":
        #             await manager.get_drones(client_id)
        #         case "get_pillars":
        #             await manager.get_pillars(client_id)
        #         case _:
                    # pass

    except WebSocketDisconnect:
        print(f"Клиент дрон станция/{client_id} отключился")
    except ValidationError as exc:
        await websocket.send_json(
            StatusMessage(status="Err", message="You lost fields").model_dump()
        )
        print(exc)
    except Exception as e:
        print(f"Ошибка WebSocket: {e}")
    finally:
        await manager.remove_connection(client_id)
