from pydantic import BaseModel
from base_models import Coordinates

class RegisterForm(BaseModel):
    coordinates: Coordinates
    radius: int
    total_drone_count: int
    total_lamps_count: int