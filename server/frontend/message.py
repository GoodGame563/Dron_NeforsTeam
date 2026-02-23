from pydantic import BaseModel
from .models import Pillar, PillarStation, DronStation
from typing import List, Any, Optional, Literal
from base_models import Coordinates


class GreetingMessage(BaseModel):
    pillars: List[Pillar]
    pillar_stations: List[PillarStation]
    dron_stations: List[DronStation]


class ChangeStatePillar(BaseModel):
    id: str
    state: str


class ChangeStateDron(BaseModel):
    id: str
    state: str
    last_coordinates: Optional[Coordinates]


class BaseMessage(BaseModel):
    event: str
    data: Any


class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""
