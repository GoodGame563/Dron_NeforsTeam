from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from pydantic import ValidationError
from models import StatusMessage, RegisterMessage, EnterMessage, GreetingMessage, BaseMessage
from managers import StationManager
import uuid
import uvicorn

app = FastAPI(
    version="0.2.0",
)


manager = StationManager("postgresql://drone_admin:12345678@localhost:5432/base")

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
    await websocket.accept()
    try:

        # await websocket.send_json(
        #         BaseMessage(
        #             event="all_data",
        #             data=GreetingMessage(

        #             )
        #         ).model_dump()
        #     )
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

