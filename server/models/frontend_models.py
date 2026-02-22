from pydantic import BaseModel
from .base_models import Coordinates
from uuid import UUID
from datetime import datetime
from typing import List

class Pillar(BaseModel):
    id: UUID
    coordinates: Coordinates
    state: str
    pillar_station_id: UUID
    last_update: datetime
    
class PillarStation(BaseModel):
    id: UUID
    coordinates: Coordinates
    is_alive: bool

class Dron(BaseModel):
    id: UUID
    status: str
    last_coordinates: Coordinates | None

class DronStation(BaseModel):
    id: UUID
    coordinates: Coordinates
    radius: int
    total_drone_count: int
    total_lamps_count: int
    drons: List[Dron]
