from datetime import datetime
from typing import Literal, Dict, List, Any
from frontend import BaseMessage, ChangeStatePillar, GreetingMessage, Pillar, PillarStation, DronStation, Dron
from pillar_station import StatusMessage as StatusMessagePillarStation
from dron_station import RegisterForm, StatusMessage as StatusMessageDronStation, PillarChangeState
from base_models import Coordinates
from fastapi import WebSocket
import asyncpg
import json
import uuid
from db_work import (
    get_id_dron_station_by_pillar,
    
)
from asyncpg import Pool

from db_work import assign_pillars_to_dron_station_and_get_count, exists_pillar, get_all_dron_stations, get_all_pillar_stations, get_all_pillars, get_dron_station, get_drons_by_station, get_pillar_station, get_pillars_for_dron_station, insert_dron, insert_dron_station, update_pillar_state


class DefaultManager:
    def __init__(self):
        self.drone_station_connections: Dict[uuid.UUID, WebSocket] = {}
        self.pillar_station_connections: Dict[uuid.UUID, WebSocket] = {}
        self.frontend_connections: List[WebSocket] = []

    async def _send_status_pillar_station(
        self, websocket: WebSocket, status: Literal["Ok", "Err"], message: str
    ):
        await websocket.send_json(
            StatusMessagePillarStation(status=status, message=message).model_dump()
        )

    async def _send_status_dron_station(
        self, websocket: WebSocket, status: Literal["Ok", "Err"], message: str
    ):
        await websocket.send_json(
            StatusMessageDronStation(status=status, message=message).model_dump()
        )

    async def _send_message_dron_station(self, websocket: WebSocket, name_event: str,data: Any):
        await websocket.send_json(
            BaseMessage(
                event= name_event, data=data
            ).model_dump()
        )
    
    async def _send_message_frontend(self, websocket: WebSocket, name_event: str,data: Any):
        await websocket.send_json(
            BaseMessage(
                event= name_event, data=data
            ).model_dump()
        )

    # async def _send_result(
    #     self, websocket: WebSocket, status: Literal["Ok", "Err"], message: List
    # ):
    #     await websocket.send_json(
    #         SendMessage(status=status, message=message).model_dump()
    #     )



class Manager(DefaultManager):
    async def broadcast_to_frontend(self, message, name_event:str):
        dead = []
        for ws in self.frontend_connections:
            try:
                await self._send_message_frontend(ws, name_event, message)
            except Exception:
                dead.append(ws)

    async def broadcast_to_dron_station(self, dron_station_id: uuid.UUID, message, name_event:str):
        websocket = self.drone_station_connections[dron_station_id]
        try:
            await self._send_message_dron_station(websocket, name_event, message)
        except Exception:
            self.drone_station_connections.pop(dron_station_id)

    async def send_first_message_frontend(self, websocket: WebSocket, pool: Pool):
        async with pool.acquire() as conn:
            await self._send_message_frontend(websocket, "all_data", GreetingMessage(
                        pillars=[
                            Pillar(
                                id=str(p.id),
                                coordinates=Coordinates(
                                    x=int(p.latitude), y=int(p.longitude)
                                ),
                                state=p.state,
                                pillar_station_id=str(p.id_pillar_station),
                                last_update=datetime.now().isoformat(),
                                id_dron_station=None if (await get_id_dron_station_by_pillar(conn, p.id)) is None else str(await get_id_dron_station_by_pillar(conn, p.id))
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
                    )
        self.frontend_connections.append(websocket)

    async def change_pillar(self, pillar_id: uuid.UUID, status: str, pool: Pool):
        async with pool.acquire() as conn:
            if not await exists_pillar(conn, pillar_id):
                return 
            await update_pillar_state(conn, pillar_id, status)
            if status == 'death' or status == 'empty':
                result = await get_id_dron_station_by_pillar(conn, pillar_id)
                if result is not None:
                    await self.broadcast_to_dron_station(result, PillarChangeState(id =str(pillar_id),status= status), "change_state_pillar")
                    
        
        await self.broadcast_to_frontend(ChangeStatePillar(id= str(pillar_id), state=status), "change_state_pillar")
        return True

    async def pillar_station_enter(self, websocket: WebSocket, client_id: str, pool: Pool):
        async with pool.acquire() as conn:
            result = await get_pillar_station(conn=conn, id_=uuid.UUID(client_id))
            if result is None:
                await self._send_status_pillar_station(websocket, "Err", "Ошибка в данных")
                await websocket.close()
                return None
            _id = uuid.UUID(client_id)
            self.pillar_station_connections[_id] = websocket
            await self._send_status_pillar_station(websocket, "Ok", "")
        return _id

    async def pillar_lamp_off(self, client_id: uuid.UUID, lamp_id: str, pool: Pool):
        async with pool.acquire() as conn:
            await update_pillar_state(conn=conn, id_=uuid.UUID(lamp_id), state="death")
            await self._send_status_pillar_station(self.pillar_station_connections[client_id], "Ok", "")

        async with pool.acquire() as conn:
            print(
                await get_id_dron_station_by_pillar(
                    conn=conn, pillar_id=uuid.UUID(lamp_id)
                )
            )

    async def dron_station_enter(self, websocket: WebSocket, client_id: str, pool: Pool):
        async with pool.acquire() as conn:
            result = await get_dron_station(conn=conn, id_=uuid.UUID(client_id))
            if result is None:
                await self._send_status_dron_station(websocket, "Err", "Ошибка в данных")
                await websocket.close()
                return None
            _id = uuid.UUID(client_id)
            self.drone_station_connections[_id] = websocket
            await self._send_status_dron_station(websocket, "Ok", "")
        return _id

    async def dron_station_register(
        self, websocket: WebSocket, message: RegisterForm, pool: Pool
    ) -> uuid.UUID | None:
        async with pool.acquire() as conn:
            client_id = await insert_dron_station(
                conn=conn,
                latitude=message.coordinates.x,
                longitude=message.coordinates.y,
                radius=message.radius,
                total_drone_count=message.total_drone_count,
                total_lamps_count=message.total_lamps_count,
            )
            
            if client_id == None:
                await self._send_status_dron_station(websocket, "Err", "Ошибка в данных")
                await websocket.close()
                return None
            _id = uuid.UUID(str(client_id))
            self.drone_station_connections[_id] = websocket
            await self._register_drons( _id, pool)
            
            
            await self._send_status_dron_station(websocket, "Ok", f"{client_id}")
            await assign_pillars_to_dron_station_and_get_count(
                conn=conn, dron_station_id=_id
            )
        return _id
    
    async def _register_drons(self, id_dron_station: uuid.UUID, pool: Pool):
        async with pool.acquire() as coon:
            d_s = await get_dron_station(conn=coon, id_=id_dron_station)
            if d_s is None:
                await self._send_status_dron_station(
                    self.drone_station_connections[id_dron_station], "Err", "Ошибка в данных"
                )
                await self.drone_station_connections[id_dron_station].close()
                return None
            i_drons = []
            for _ in range(
                int(d_s.total_drone_count)
                - len(await get_drons_by_station(conn=coon, id_dron_station=id_dron_station))
            ):
                i_drons.append(
                    str(await insert_dron(conn=coon, id_dron_station=id_dron_station))
                )
            await self._send_status_dron_station(
                self.drone_station_connections[id_dron_station], "Ok", json.dumps(i_drons)
            )

    async def get_drones(self, client_id: uuid.UUID, pool: Pool):
        async with pool.acquire() as coon:
            drons: List[Dron] = []
            for d in await get_drons_by_station(conn=coon, id_dron_station=client_id):
                drons.append(
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
                )
            await self._send_message_dron_station(self.drone_station_connections[client_id], "get_drons", drons)

   

    async def get_pillars(self, client_id: uuid.UUID, pool: Pool):
        async with pool.acquire() as conn:

            p_d_s = await get_pillars_for_dron_station(
                conn=conn, dron_station_id=client_id
            )
            if p_d_s is None:
                await self._send_status_dron_station(
                    self.drone_station_connections[client_id], "Err", "Ошибка в данных"
                )
                await self.drone_station_connections[client_id].close()
                return None
            pillars = []
            for p in p_d_s:
                pillars.append(
                    Pillar(
                        id=str(p.pillar_id),
                        coordinates=Coordinates(x =int(p.x), y= int(p.y)),
                        state=p.state,
                        pillar_station_id=str(p.id_pillar_station),
                        id_dron_station=str(client_id),
                        last_update=datetime.now().isoformat()
                    )
                )
            await self._send_message_dron_station(self.drone_station_connections[client_id], "get_pillars", pillars)
