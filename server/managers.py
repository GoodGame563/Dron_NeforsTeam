from typing import Literal, Dict, List, Any
from models import ClientType, GetPillarsMessage, StatusMessage, PillarStatusMessage, RegisterMessage, DronMessage, Coordinates, SendMessage, PillarMessage
from fastapi import FastAPI, WebSocket, WebSocketDisconnect, HTTPException
import asyncpg
import json
import uuid
from db_work import insert_dron_station, get_dron_station, get_drons_by_station, insert_dron, assign_pillars_to_dron_station_and_get_count, get_pillars_for_dron_station

class DefaultManager:
    def __init__(self, connection_url: str):
        self.connections: Dict[uuid.UUID, WebSocket] = {}
        self.db_url = connection_url
        self.pool:asyncpg.Pool|None = None
        self.client_id = ""

    async def _send_status(
        self, 
        websocket: WebSocket, 
        status: Literal["Ok", "Err"],  
        message: str
    ):
        await websocket.send_json(
            StatusMessage(status=status, message=message).model_dump()
        )
    async def _send_result(
        self, 
        websocket: WebSocket, 
        status: Literal["Ok", "Err"],  
        message: List
    ):
        await websocket.send_json(
            SendMessage(status=status, message=message).model_dump()
        )


    async def remove_connection(self, id):
        self.connections.pop(id, None)

class DronStationManager(DefaultManager):
    async def enter(self, websocket: WebSocket, client_id: str):
        pool = await asyncpg.create_pool(dsn = self.db_url)
        async with pool.acquire() as conn:
            result = await get_dron_station(conn=conn, id_ =uuid.UUID(client_id))
            if result is None:
                await self._send_status(websocket, "Err", "Ошибка в данных")
                await websocket.close()
                return None
            _id = uuid.UUID(client_id)
            self.connections[_id] = websocket
            await self._send_status(websocket, "Ok", "")
        self.pool = pool
        self.client_id = _id
        return client_id

    async def register(self, websocket: WebSocket, message: RegisterMessage) -> str|None:
        pool = await asyncpg.create_pool(dsn = self.db_url)
        async with pool.acquire() as conn:
            client_id = await insert_dron_station(conn=conn, latitude=message.coordinates.latitude, longitude=message.coordinates.longtiude, radius=message.radius, total_drone_count=message.total_drone_count, total_lamps_count=message.total_lamps_count)
            if client_id == None:
                await self._send_status(websocket, "Err", "Ошибка в данных")
                await websocket.close()
                return None
            _id = uuid.UUID(str(client_id))
            self.connections[_id] = websocket
            await self._send_status(websocket, "Ok", f"{client_id}")
        self.pool = pool
        self.client_id = _id 
        return str(client_id)
    
    async def get_drones(self):
        if self.pool is None: 
            await self.connections[self.client_id].close()
            return
        async with self.pool.acquire() as coon:
            drons:List[DronMessage] = []
            for d in await get_drons_by_station(conn=coon, id_dron_station = self.client_id):
                drons.append(
                    DronMessage(
                        id=str(d['id']),
                        status=d['status'],
                        last_coordinates= Coordinates(
                            latitude=d['last_latitude'],
                            longtiude=d['last_longitude']
                        ) if d['last_longitude'] is not None
                        else None
                    )
                )
            await self._send_result(self.connections[self.client_id], "Ok", drons)

    async def register_drons(self):
        if self.pool is None: 
            await self.connections[self.client_id].close()
            return
        async with self.pool.acquire() as coon:
            d_s = await get_dron_station(conn = coon, id_=self.client_id)
            if d_s is None:
                await self._send_status(self.connections[self.client_id], "Err", "Ошибка в данных")
                await self.connections[self.client_id].close()
                return None 
            i_drons = []
            for _ in range(int(d_s["total_drone_count"]) - len(await get_drons_by_station(conn=coon, id_dron_station = self.client_id))):
                i_drons.append(str(await insert_dron(conn=coon, id_dron_station=self.client_id)))
            await self._send_status(self.connections[self.client_id], "Ok", json.dumps(i_drons))

    async def get_pillars(self):
        if self.pool is None: 
            await self.connections[self.client_id].close()
            return
        async with self.pool.acquire() as conn:
            await assign_pillars_to_dron_station_and_get_count(conn=conn, dron_station_id=self.client_id)
            p_d_s = await get_pillars_for_dron_station(conn=conn, dron_station_id=self.client_id)
            if p_d_s is None:
                await self._send_status(self.connections[self.client_id], "Err", "Ошибка в данных")
                await self.connections[self.client_id].close()
                return None 
            pillars = []
            for p in p_d_s:
                pillars.append(PillarMessage(
                    pillar_id=str(p['pillar_id']),
                    x= p['x'],
                    y =p['y'],
                    state=p['state'],
                    pillar_station_id=str(p['id_pillar_station'])
                ))
            await self._send_result(self.connections[self.client_id], "Ok", pillars)



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

        if client_type == ClientType.PILLAR_STATION:
            if not client_id:
                await self._send_status(websocket, "Err", "client_id обязателен для pillar")
                await websocket.close()
                return
            self.pillars[client_id] = websocket
            await self._send_pillars(websocket)

        elif client_type == ClientType.DRONE_STATION:
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