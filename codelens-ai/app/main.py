from dotenv import load_dotenv
load_dotenv()

from fastapi import FastAPI
from contextlib import asynccontextmanager
import asyncio
from app.worker.worker import start_worker
from app.worker.summary_worker import start_summary_worker
from app.routers import search

@asynccontextmanager
async def lifespan(app:FastAPI):
  task = asyncio.create_task(start_worker())
  summary_task = asyncio.create_task(start_summary_worker())
  yield
  task.cancel()
  summary_task.cancel()
  try:
    await task
  except asyncio.CancelledError:
    pass
  try:
    await summary_task
  except asyncio.CancelledError:
    pass

app = FastAPI(title ="CodeLens AI service", lifespan=lifespan)

app.include_router(search.router)

@app.get("/health")
def health():
  return {"status": "ok"}