from pydantic import BaseModel

class MessageDto(BaseModel):
    role: str
    content: str

class ChunkDto(BaseModel):
    file_path: str
    content: str
    start_line: int
    end_line: int
    similarity: float

class TokenUsage(BaseModel):
    prompt_tokens:int
    completion_tokens:int
    total_tokens:int

class SearchRequest(BaseModel):
    query: str
    repoId:str
    history: list[MessageDto] = []
    userId:str

class SearchResponse(BaseModel):
    answer: str
    chunks: list[ChunkDto]
    usage: TokenUsage

