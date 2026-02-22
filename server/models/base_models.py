from pydantic import BaseModel

class Coordinates(BaseModel):
    x: int
    y: int