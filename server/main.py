from fastapi import FastAPI, WebSocket, WebSocketDisconnect, HTTPException
from pydantic import BaseModel
from typing import Literal, Dict, List, Any
from enum import Enum
import logging

app = FastAPI()
logger = logging.getLogger()

class ClientType(str, Enum):
    DRONE = "drone_station"
    PILLAR = "pillar_station"
    FRONTEND = "frontend"

class RegisterMessage(BaseModel):
    event: Literal["register"] = "register"
    client_type: ClientType
    client_id: str | None = None

class DroneStateMessage(BaseModel):
    event: Literal["dron_state"] = "dron_state"
    drone_id: str
    drone_status: Literal["in_station", "fly", "broken"]
    pilar_id: str
    last_coordinatination: Dict[str, float]

class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""

class PillarStatusMessage(BaseModel):
    event: Literal["pillar_status"] = "pillar_status"
    id_pillar: str

class GetPillarsMessage(BaseModel):
    event: Literal["get_pillars"] = "get_pillars"
    pillars: List[Dict[str, Any]]

class ConnectionManager:
    def __init__(self):
        self.drones: Dict[str, WebSocket] = {}
        self.pillars: Dict[str, WebSocket] = {}
        self.frontends: List[WebSocket] = []


        self.pillar_info: Dict[str, dict] = {
            "pillar1": {"x": 55.751244, "y": 37.618423},
            "pillar2": {"x": 55.755, "y": 37.62},
            "pillar3": {"x": 55.76, "y": 37.63},
        }
        self.drone_states: Dict[str, dict] = {}

    async def register(self, websocket: WebSocket, client_type: ClientType, client_id: str | None):

        if client_type == ClientType.PILLAR:
            if not client_id:
                await self._send_status(websocket, "Err", "client_id обязателен для pillar")
                await websocket.close()
                return
            self.pillars[client_id] = websocket
            await self._send_pillars(websocket)

        elif client_type == ClientType.DRONE:
            if not client_id:
                await self._send_status(websocket, "Err", "client_id обязателен для drone")
                await websocket.close()
                return
            self.drones[client_id] = websocket

        elif client_type == ClientType.FRONTEND:
            self.frontends.append(websocket)
            await self._send_pillars(websocket)             

        await self._send_status(websocket, "Ok", f"{client_type} успешно подключён")

    async def _send_pillars(self, websocket: WebSocket):
        pillars_list = [
            {"id": pid, "x": info["x"], "y": info["y"]}
            for pid, info in self.pillar_info.items()
        ]
        await websocket.send_json(GetPillarsMessage(pillars=pillars_list).model_dump())

    async def _send_status(
        self, 
        websocket: WebSocket, 
        status: Literal["Ok", "Err"],  
        message: str
    ):
        await websocket.send_json(
            StatusMessage(status=status, message=message).model_dump()
        )

    async def broadcast_to_frontends(self, message: dict):
        dead = []
        for ws in self.frontends:
            try:
                await ws.send_json(message)
            except Exception:
                dead.append(ws)
        for d in dead:
            if d in self.frontends:
                self.frontends.remove(d)

    async def broadcast_pillar_status(self, pillar_id: str):
        msg = PillarStatusMessage(id_pillar=pillar_id).model_dump()

        await self.broadcast_to_frontends(msg)

        for ws in list(self.drones.values()):
            try:
                await ws.send_json(msg)
            except:
                pass

        for ws in list(self.pillars.values()):
            try:
                await ws.send_json(msg)
            except:
                pass

    async def remove_connection(self, websocket: WebSocket, client_type: str | None, client_id: str | None):
        if client_type == "drone" and client_id:
            self.drones.pop(client_id, None)
        elif client_type == "pillar" and client_id:
            self.pillars.pop(client_id, None)
        elif client_type == "frontend":
            if websocket in self.frontends:
                self.frontends.remove(websocket)

manager = ConnectionManager()

@app.websocket("/ws")
async def websocket_endpoint(websocket: WebSocket):
    client_type: str | None = None
    client_id: str | None = None
    await websocket.accept()
    try:
        data = await websocket.receive_json()
        if data.get("event") != "register":
            await websocket.send_json(StatusMessage(status="Err", message="Первое сообщение должно быть register").model_dump())
            await websocket.close(code=1003)
            return

        reg = RegisterMessage(**data)
        client_type = reg.client_type.value
        client_id = reg.client_id

        await manager.register(websocket, reg.client_type, client_id)

        while True:
            data = await websocket.receive_json()
            event = data.get("event")
            if event == "status":
                logger.info(f"Клиент {client_type}/{client_id} ответил статусом: {data}")
                continue

            if event == "dron_state" and client_type == "drone":
                try:
                    state = DroneStateMessage(**data)
                    manager.drone_states[state.drone_id] = data

                    await manager._send_status(websocket, "Ok", "Состояние сохранено")

                    await manager.broadcast_to_frontends(data)


                    if state.drone_status == "broken":
                        await manager.broadcast_pillar_status(state.pilar_id)

                except Exception as e:
                    await manager._send_status(websocket, "Err", str(e))

            else:
                await manager._send_status(websocket, "Err", f"Неизвестное событие: {event}")

    except WebSocketDisconnect:
        logger.info(f"Клиент {client_type}/{client_id} отключился")
    except Exception as e:
        logger.error(f"Ошибка WebSocket: {e}")
    finally:
        await manager.remove_connection(websocket, client_type, client_id)


@app.post("/break_pillar/{pillar_id}")
async def break_pillar(pillar_id: str):
    """Искусственно ломаем столб — сервер отправляет pillar_status всем клиентам"""
    if pillar_id not in manager.pillar_info:
        raise HTTPException(status_code=404, detail="Столб не найден")

    await manager.broadcast_pillar_status(pillar_id)
    return {"status": "Ok", "message": f"Уведомление о поломке столба {pillar_id} отправлено всем клиентам"}


# if __name__ == "__main__":
#     uvicorn.run(app, host="0.0.0.0", port=8000, log_level="info")