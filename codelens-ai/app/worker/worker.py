import asyncio
import os
import redis.asyncio as aioredis
from sqlalchemy import select
from app.db.session import AsyncSessionLocal
from app.models.models import User, Repository, RepositoryFile, FileChunk
from app.services.fetchContents import fetch_contents, FetchStatus
from app.services.is_indexable import is_indexable
from app.services.fetch_tokens import fetch_tokens
from app.services.decrypt import decrypt_token
from app.services.chunking import chunk_file
from app.services.embedder import embed_chunks
from sqlalchemy.orm import selectinload

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
STREAM_NAME = "indexing-jobs"
GROUP_NAME = "indexing-group"
CONSUMER_NAME = "worker-1"

async def process_job(repo_id: str, user_id: str):
    print("process started")
    async with AsyncSessionLocal() as db:
        files = await db.execute(
            select(RepositoryFile)
            .where(RepositoryFile.repository_id == repo_id)
            .options(selectinload(RepositoryFile.repository))
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

        githubusername = user.github_username
        token = user.github_access_token
        decrypted_token = decrypt_token(token)
        repo = files[0].repository.full_name.split("/")[1]

        for file in files:
            if not is_indexable(file.path):
                continue

            status, content = await fetch_contents(decrypted_token, githubusername, repo, file.path)

            if status == FetchStatus.UNAUTHORIZED:
                flag = await fetch_tokens(user_id)
                if not flag:
                    print(f"Token refresh failed for user {user_id}")
                    return

                user_result = await db.execute(select(User).where(User.id == user_id))
                user = user_result.scalar_one_or_none()
                decrypted_token = decrypt_token(user.github_access_token)

                status, content = await fetch_contents(decrypted_token, githubusername, repo, file.path)

                if status != FetchStatus.OK or content is None:
                    print(f"Retry failed for {file.path}")
                    continue

            if status == FetchStatus.ERROR or content is None:
                print(f"Skipping {file.path}")
                continue

            chunks = chunk_file(content, file.path)
            embedded = embed_chunks(chunks)

            for chunk, vector in embedded:
                db_chunk = FileChunk(
                    repository_file_id=file.id,
                    content=chunk.content,
                    embedding=vector,
                    chunk_index=chunk.chunk_index,
                    start_line=chunk.start_line,
                    end_line=chunk.end_line,
                )
                db.add(db_chunk)

        await db.commit()
        print(f"files processed for {repo_id}")

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
                    break

                if not messages:
                    continue

                for _, entries in messages:
                    await _process_entries(r, entries)

        except Exception as e:
            print(f"Worker connection error: {e}, retrying in 5s...")
            await asyncio.sleep(5)