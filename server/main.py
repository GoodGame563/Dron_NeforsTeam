from fastapi import FastAPI, WebSocket, WebSocketDisconnect
from pydantic import ValidationError
from models import StatusMessage, RegisterMessage, EnterMessage
from managers import StationManager
import uuid

app = FastAPI()


manager = StationManager("postgresql://drone_admin:12345678@localhost:5432/base")

# @app.websocket("/ws")
# async def websocket_endpoint(websocket: WebSocket):
#     client_type: str | None = None
#     client_id: str | None = None
#     await websocket.accept()
#     try:
#         data = await websocket.receive_json()
#         if data.get("event") != "register":
#             await websocket.send_json(StatusMessage(status="Err", message="Первое сообщение должно быть register").model_dump())
#             await websocket.close(code=1003)
#             return

#         reg = RegisterMessage(**data)
#         client_type = reg.client_type.value
#         client_id = reg.client_id

#         await manager.register(websocket, reg.client_type, client_id)

#         while True:
#             data = await websocket.receive_json()
#             event = data.get("event")
#             # if event == "status":
#             #     logger.info(f"Клиент {client_type}/{client_id} ответил статусом: {data}")
#             #     continue

#             # if event == "dron_state" and client_type == "drone":
#             #     try:
#             #         state = DroneStateMessage(**data)
#             #         manager.drone_states[state.drone_id] = data

#             #         await manager._send_status(websocket, "Ok", "Состояние сохранено")

#             #         await manager.broadcast_to_frontends(data)


#             #         if state.drone_status == "broken":
#             #             await manager.broadcast_pillar_status(state.pilar_id)

#             #     except Exception as e:
#             #         await manager._send_status(websocket, "Err", str(e))

#             # else:
#             #     await manager._send_status(websocket, "Err", f"Неизвестное событие: {event}")

#     except WebSocketDisconnect:
#         print(f"Клиент {client_type}/{client_id} отключился")
#     except Exception as e:
#         print(f"Ошибка WebSocket: {e}")
#     finally:
#         await manager.remove_connection(websocket, client_type, client_id)


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
                case "get_drons":
                    await manager.get_drones(client_id)
                case "get_pillars":
                    await manager.get_pillars(client_id)
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


# @app.post("/break_pillar/{pillar_id}")
# async def break_pillar(pillar_id: str):
#     """Искусственно ломаем столб — сервер отправляет pillar_status всем клиентам"""
#     if pillar_id not in manager.pillar_info:
#         raise HTTPException(status_code=404, detail="Столб не найден")

#     await manager.broadcast_pillar_status(pillar_id)
#     return {"status": "Ok", "message": f"Уведомление о поломке столба {pillar_id} отправлено всем клиентам"}


# if __name__ == "__main__":
#     uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")
