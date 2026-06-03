from fastapi import FastAPI
from contextlib import asynccontextmanager
import asyncio
from app.worker.worker import start_worker
from app.routers import search

@asynccontextmanager
async def lifespan(app:FastAPI):
  task = asyncio.create_task(start_worker())
  yield
  task.cancel()
  try:
    await task
  except asyncio.CancelledError:
    pass

app = FastAPI(title ="CodeLens AI service", lifespan=lifespan)

app.include_router(search.router)

@app.get("/health")
def health():
  return {"status": "ok"}