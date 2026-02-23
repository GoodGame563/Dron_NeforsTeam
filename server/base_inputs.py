from pydantic import BaseModel
from typing import Any


class EnterData(BaseModel):
    event: str
    data: Any


class EnterForm(BaseModel):
    id: str
