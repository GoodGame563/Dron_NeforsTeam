from pydantic import BaseModel
from typing import Any

class Coordinates(BaseModel):
    x: int
    y: int

