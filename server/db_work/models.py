import uuid
from dataclasses import dataclass
from datetime import datetime
from typing import Optional


@dataclass
class PillarStation:
    id: uuid.UUID
    latitude: float
    longitude: float
    is_alive: bool


@dataclass
class DronStation:
    id: uuid.UUID
    latitude: float
    longitude: float
    radius: int
    total_drone_count: int
    total_lamps_count: int


@dataclass
class Pillar:
    id: uuid.UUID
    latitude: float
    longitude: float
    state: str  # 'alive', 'death', 'empty'
    id_pillar_station: uuid.UUID


@dataclass
class PillarToDronStation:
    id: uuid.UUID
    id_pillar: uuid.UUID
    id_dron_station: uuid.UUID


@dataclass
class Dron:
    id: uuid.UUID
    status: str  # 'in_station', 'fly', 'broken'
    last_latitude: Optional[float]
    last_longitude: Optional[float]
    id_dron_station: Optional[uuid.UUID]


@dataclass
class History:
    id: uuid.UUID
    id_dron_station: uuid.UUID
    id_pillars: uuid.UUID
    id_dron: uuid.UUID
    status: str  # 'broken', 'clear', 'work', 'done'
    update_at: datetime


@dataclass
class PillarForStation:
    """Результат get_pillars_for_dron_station_flat"""

    pillar_id: uuid.UUID
    x: float  # longitude
    y: float  # latitude
    state: str
    id_pillar_station: uuid.UUID
