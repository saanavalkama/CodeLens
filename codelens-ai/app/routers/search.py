from fastapi import APIRouter, Header, HTTPException
from app.models.search import SearchRequest, SearchResponse
from app.services.search import SearchService
from openai import AuthenticationError, APIStatusError
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
    try:
        answer = await _service.search(request)
    except AuthenticationError:
        raise HTTPException(status_code=502, detail="OpenAI authentication failed — check OPENAI_API_KEY")
    except APIStatusError as e:
        raise HTTPException(status_code=502, detail=f"OpenAI API error: {e.message}")
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Search failed: {str(e)}")
    return answer