from pydantic import BaseModel
from typing import Any

class Answer(BaseModel):
    event:str
    data: Any