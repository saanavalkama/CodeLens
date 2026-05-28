import asyncio
from app.db.session import engine
from app.models.models import Base
from sqlalchemy import text

async def init_db():
    async with engine.begin() as conn:
        # enable pgvector extension first
        await conn.execute(text("CREATE EXTENSION IF NOT EXISTS vector"))
        await conn.run_sync(Base.metadata.create_all)

if __name__ == "__main__":
    asyncio.run(init_db())