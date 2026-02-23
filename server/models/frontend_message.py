from pydantic import BaseModel
from .frontend_models import Pillar, PillarStation, DronStation
from typing import List, Any


class GreetingMessage(BaseModel):
    pillars: List[Pillar]
    pillar_stations: List[PillarStation]
    dron_stations: List[DronStation]


class BaseMessage(BaseModel):
    event: str
    data: Any
