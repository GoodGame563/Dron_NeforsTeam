from pydantic import BaseModel
from .models import Pillar, PillarStation, DronStation
from typing import List, Any


class GreetingMessage(BaseModel):
    pillars: List[Pillar]
    pillar_stations: List[PillarStation]
    dron_stations: List[DronStation]

class ChangeStatePillar(BaseModel):
    id: str
    state: str

class BaseMessage(BaseModel):
    event: str
    data: Any
