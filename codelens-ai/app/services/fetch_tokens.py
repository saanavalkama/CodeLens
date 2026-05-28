import httpx
import os

INTERNAL_API_URL = os.getenv("INTERNAL_API_URL")
INTERNAL_API_KEY= os.getenv("INTERNAL_API_KEY")

async def fetch_tokens(userId:str)->bool:
    async with httpx.AsyncClient() as client:
        url = f"{INTERNAL_API_URL}/internal/refresh-token/{userId}"
        headers = {'X-Internal-Key':INTERNAL_API_KEY}
        response = await client.post(url,headers=headers)
        return response.status_code == 204

