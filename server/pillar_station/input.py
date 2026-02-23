from pydantic import BaseModel
from typing  import Any

class ChangeForm(BaseModel):
    id: str
    status: str