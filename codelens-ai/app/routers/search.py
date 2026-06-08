from fastapi import APIRouter, Header, HTTPException
from app.models.search import SearchRequest, SearchResponse
from app.services.search import SearchService
import os

router = APIRouter(prefix="/search")

INTERNAL_API_KEY = os.getenv("INTERNAL_API_KEY")
_service = SearchService()

@router.post("")
async def search(
    request: SearchRequest,
    x_internal_api_key: str = Header(...)
) -> SearchResponse:
    if x_internal_api_key != INTERNAL_API_KEY:
        raise HTTPException(status_code=401, detail="Unauthorized")
    answer = await _service.search(request)
    return SearchResponse(answer=answer)