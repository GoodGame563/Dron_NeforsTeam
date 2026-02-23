from pydantic import BaseModel
from typing import Literal


class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""


class SetModePillar(BaseModel):
    id: str
    is_hit: bool
    is_service: bool
