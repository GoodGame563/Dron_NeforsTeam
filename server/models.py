from pydantic import BaseModel
from enum import Enum
from typing import Literal, Dict, List, Any


class ClientType(str, Enum):
    DRONE_STATION = "drone_station"
    PILLAR_STATION = "pillar_station"
    FRONTEND = "frontend"


class DronStatus(str, Enum):
    STATION = "in_station"
    WORK = "fly"
    BROKEN = "broken"


class Coordinates(BaseModel):
    latitude: float
    longtiude: float


class RegisterMessage(BaseModel):
    event: Literal["register"] = "register"
    coordinates: Coordinates
    radius: int
    total_drone_count: int
    total_lamps_count: int


class EnterMessage(BaseModel):
    event: Literal["enter"]
    client_id: str


class Drons(BaseModel):
    id: str
    status: DronStatus
    last_coordinates: Coordinates | None = None


class DronMessage(BaseModel):
    id: str
    status: str
    last_coordinates: Coordinates | None = None


class SetDrons(BaseModel):
    event: Literal["set_drons"]
    drons: List[Drons]


class DroneStateMessage(BaseModel):
    event: Literal["dron_state"] = "dron_state"
    drone_id: str
    drone_status: Literal["in_station", "fly", "broken"]
    pilar_id: str
    last_coordinates: Dict[str, float]


class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""


class SendMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: List


class PillarStatusMessage(BaseModel):
    event: Literal["pillar_status"] = "pillar_status"
    id_pillar: str


class PillarMessage(BaseModel):
    pillar_id: str
    x: float
    y: float
    state: str
    pillar_station_id: str


class GetPillarsMessage(BaseModel):
    event: Literal["get_pillars"] = "get_pillars"
    pillars: List[Dict[str, Any]]
