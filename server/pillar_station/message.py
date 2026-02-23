from pydantic import BaseModel
from typing import Literal, Dict, List, Any

class StatusMessage(BaseModel):
    event: Literal["status"] = "status"
    status: Literal["Ok", "Err"]
    message: str = ""
