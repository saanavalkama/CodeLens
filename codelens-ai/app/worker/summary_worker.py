import os
import redis.asyncio as aioredis
import asyncio
from app.db.session import AsyncSessionLocal
from app.models import Repository, RepositoryFile, User
from sqlalchemy import select
from app.services.decrypt import decrypt_token
from openai import AsyncOpenAI
import json

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
CONSUMER_NAME = "summary-worker-1"
SUMMARY_STREAM_NAME = "repo-summary-jobs"
SUMMARY_GROUP_NAME = "summary-group"

async def create_summary(repoId:str, userId:str):
    
    async with AsyncSessionLocal() as db:
        result = await db.execute(
            select(Repository)
            .where(Repository.id == repoId)
        )

    repo = result.scalar_one_or_none()

    if not repo:
        return 
    
    if repo.summary is not None:
        return 

    async with AsyncSessionLocal() as file_db:
        files_result = await file_db.execute(
            select(RepositoryFile)
            .where(RepositoryFile.repository_id == repoId)
        )
    files = files_result.scalars().all()
        
    def get_folder_depth(path, depth=2):
        parts = path.split("/")
        return "/".join(parts[:depth])

    folders = sorted(set(get_folder_depth(f.path) for f in files))
    file_tree = "\n".join(folders)
   
    system_prompt = """You are an expert software architect analyzing a codebase.
        Your goal is to produce a concise architectural summary covering:
        - What this project does
        - Main technologies and frameworks used 
        - Key components and how they interact
        - Notable patterns (auth, caching, async processing, etc)

        You have tools to explore the codebase. Start by reading entry point files
        like Program.cs, main.py, or package.json. Follow interesting threads.
        When you have enough context, write the summary directly as text.
        Do not call any more tools once you have enough information."""

    initial_messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": f"Analyze this codebase. Here is the folder structure:\n\n{file_tree}"}
    ]

    api_key = _get_openai_key(userId)

    if not api_key:return 

    client = AsyncOpenAI(api_key=api_key)

    tools = {}

    max_iterations = 10

    for x in range(max_iterations):

        if x == max_iterations - 1:
            initial_messages.append({
                "role": "user",
                "content": "You have reached the maximum number of tool calls. Please write the summary now based on what you have gathered so far."
            })

        response = await client.chat.completions.create(
            model="gpt-4o-mini",
            messages=initial_messages,
            tools=tools,
            tool_choice=auto
        )
        choice = response.choices[0]

        if choice.finish_reason == "tool_calls":
            # append assistant message with tool calls
            initial_messages.append(choice.message)

            # execute each tool call
            for tool_call in choice.message.tool_calls:
                tool_name = tool_call.function.name
                args = json.loads(tool_call.function.arguments)

                result = await execute_tool(tool_name, args, repo_id)

                # append tool result
                initial_messages.append({
                    "role": "tool",
                    "tool_call_id": tool_call.id,
                    "content": result
                })

        else:
            # agent is done
            summary = choice.message.content
            async with AsyncSessionLocal() as db:
                await db.execute(
                    text('UPDATE "Repositories" SET "Summary" = :summary, "SummaryGeneratedAt" = :ts WHERE "Id" = :id'),
                    {"summary": summary, "ts": datetime.utcnow(), "id": uuid.UUID(repo_id)}
                )
                await db.commit()
            return

    # hit iteration limit — store whatever we have
    print(f"Summary agent hit max iterations for repo {repo_id}")



async def _get_openai_key(self, user_id: str) -> str:
    async with AsyncSessionLocal() as db:
        result = await db.execute(
            select(User.encrypted_openai_key).where(User.id == uuid.UUID(user_id))
        )
        encrypted = result.scalar_one_or_none()
        if not encrypted:
            return
        return decrypt_token(encrypted)


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