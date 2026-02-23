from pydantic import BaseModel


class ChangeForm(BaseModel):
    id: str
    status: str
