from pydantic import BaseModel
from typing import Literal, Dict, List, Any
from frontend import Pillar, PillarStation

class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""

class PillarChangeState(BaseModel):
    id: str
    status: str

class AllStruct(BaseModel):
    pillars: List[Pillar]
    pillar_stations: List[PillarStation]