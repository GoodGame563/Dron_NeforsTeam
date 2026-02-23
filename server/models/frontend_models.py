from pydantic import BaseModel
from .base_models import Coordinates
from typing import List


class Pillar(BaseModel):
    id: str
    coordinates: Coordinates
    state: str
    pillar_station_id: str
    last_update: str


class PillarStation(BaseModel):
    id: str
    coordinates: Coordinates
    is_alive: bool


class Dron(BaseModel):
    id: str
    status: str
    last_coordinates: Coordinates | None


class DronStation(BaseModel):
    id: str
    coordinates: Coordinates
    radius: int
    total_drone_count: int
    total_lamps_count: int
    drons: List[Dron]
