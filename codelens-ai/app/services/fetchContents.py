import httpx
import base64
from enum import Enum
from typing import Optional

class FetchStatus(Enum):
    OK ="ok"
    UNAUTHORIZED = "unauthorized"
    ERROR = "error"

async def fetch_contents(token:str, username:str,repo:str, file_path:str)->tuple[FetchStatus, Optional[str]]:
    api_url = f"https://api.github.com/repos/{username}/{repo}/contents/{file_path}"
    async with httpx.AsyncClient() as client:
        try:
            headers = {'Authorization':f"Bearer {token}", 'User-Agent':"CodeLens/1.0"}
            response = await client.get(api_url,headers=headers)

            if response.status_code == 401:
                return FetchStatus.UNAUTHORIZED, None
            
            response.raise_for_status()
            data = response.json()

            if data.get("type") != "file":
                return None

            content = base64.b64decode(data["content"]).decode("utf-8")
            return FetchStatus.OK, content
        except httpx.HTTPError as exc:
            print(f"Error while requesting {exc.request.url}")
            return FetchStatus.ERROR, None
            


