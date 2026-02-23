from pydantic import BaseModel
from base_models import Coordinates
from typing import Optional


class RegisterForm(BaseModel):
    coordinates: Coordinates
    radius: int
    total_drone_count: int
    total_lamps_count: int


class DronStateChange(BaseModel):
    id: str
    status: str
    last_coordinates: Optional[Coordinates]
