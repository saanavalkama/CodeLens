import os
import redis.asyncio as aioredis
import asyncio

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
CONSUMER_NAME = "summary-worker-1"
SUMMARY_STREAM_NAME = "repo-summary-jobs"
SUMMARY_GROUP_NAME = "summary-group"

async def create_summary(repoId:str, userId:str):
    pass

async def _process_entries(r, entries):
    for entry_id, data in entries:
        repo_id_bytes = data.get(b"repoId")
        user_id_bytes = data.get(b"userId")
        if not repo_id_bytes or not user_id_bytes:
            print(f"Missing repoId or userId in entry {entry_id}:{data} ")
            await r.xack(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, entry_id)
            continue
        repo_id = repo_id_bytes.decode()
        userId = user_id_bytes.decode()
        try: 
            await create_summary(repo_id, userId)
            await r.xack(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, entry_id)
        except Exception as e:
            print(f"Exception e while processing sumary:{e}")
    
async def start_woker():
    while True:
        try:
            r = await aioredis.from_url(REDIS_URL)
            try:
                await r.xgroup_create(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, id="0", mkstream=True)
            except Exception:
                pass

            print("summary worker started, listening for jobs")
        
            pending = await r.xreadgroup(SUMMARY_GROUP_NAME, CONSUMER_NAME, {SUMMARY_STREAM_NAME:"0"}, count=100)

            if pending:
                for _, entries in pending:
                    print(f"Recovering {len(entries)} pending summary jobs")
                    await _process_entries(r, entries)

            while True:
                try: 
                    messages = await r.xreadgroup(
                        SUMMARY_GROUP_NAME,
                        CONSUMER_NAME,
                        {SUMMARY_STREAM_NAME:">"},
                        block=5000,
                        count=1
                    )
                except Exception as e:
                    print(f"xreadgroup error: {e}")
                    break

                if not messages:
                    continue

                for _,entries in messages:
                    await _process_entries(r,entries)

        except Exception as e:
            print(f"Worker connection error: {e}, retrying in 5s")
            await asyncio.sleep(5)