from pydantic import BaseModel

class MessageDto(BaseModel):
    role: str
    content: str

class SearchRequest(BaseModel):
    query: str
    repoId:str
    history: list[MessageDto] = []
    userId:str

class SearchResponse(BaseModel):
    answer: str