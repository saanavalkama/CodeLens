import asyncio
import os
import redis.asyncio as aioredis
from sqlalchemy import select
from app.db.session import AsyncSessionLocal
from app.models.models import User, Repository, RepositoryFile

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
STREAM_NAME = "indexing-jobs"
GROUP_NAME = "indexing-group"
CONSUMER_NAME = "worker-1"

async def process_job(repo_id: str, user_id: str):
    print("process started")
    async with AsyncSessionLocal() as db:
        files = await db.execute(
            select(RepositoryFile).where(RepositoryFile.repository_id == repo_id)
        )
        files = files.scalars().all()

        user = await db.execute(
            select(User).where(User.id == user_id)
        )
        user = user.scalar_one_or_none()

        if not user or not files:
            print(f"No user or files found for repo {repo_id}")
            return

        print(f"Indexing {len(files)} files for repo {repo_id}")
        # chunking + embedding goes here next

async def _process_entries(r, entries):
    for entry_id, data in entries:
        repo_id = data[b"repoId"].decode()
        user_id = data[b"userId"].decode()
        try:
            await process_job(repo_id, user_id)
            await r.xack(STREAM_NAME, GROUP_NAME, entry_id)
        except Exception as e:
            print(f"Failed to process job {entry_id}: {e}")

async def start_worker():
    while True:
        try:
            r = await aioredis.from_url(REDIS_URL)

            try:
                await r.xgroup_create(STREAM_NAME, GROUP_NAME, id="0", mkstream=True)
            except Exception:
                pass

            print("Worker started, listening for jobs...")

            # drain any pending messages from a previous crashed run
            pending = await r.xreadgroup(
                GROUP_NAME, CONSUMER_NAME, {STREAM_NAME: "0"}, count=100
            )
            if pending:
                for _, entries in pending:
                    print(f"Recovering {len(entries)} pending message(s)")
                    await _process_entries(r, entries)

            while True:
                try:
                    messages = await r.xreadgroup(
                        GROUP_NAME,
                        CONSUMER_NAME,
                        {STREAM_NAME: ">"},
                        block=5000,
                        count=1,
                    )
                except Exception as e:
                    print(f"xreadgroup error: {e}")
                    break  # reconnect outer loop

                if not messages:
                    continue

                for _, entries in messages:
                    await _process_entries(r, entries)

        except Exception as e:
            print(f"Worker connection error: {e}, retrying in 5s...")
            await asyncio.sleep(5)