import os
import uuid
import json
from datetime import datetime
import redis.asyncio as aioredis
import asyncio
from sqlalchemy import select, text
from app.db.session import AsyncSessionLocal
from app.models.models import Repository, RepositoryFile, User
from app.services.decrypt import decrypt_token
from app.services.tool_functions import read_file as _read_file, list_files as _list_files, search as _search
from openai import AsyncOpenAI

REDIS_URL = os.getenv("REDIS_URL", "redis://localhost:6379")
CONSUMER_NAME = "summary-worker-1"
SUMMARY_STREAM_NAME = "repo-summary-jobs"
SUMMARY_GROUP_NAME = "summary-group"

TOOLS = [
    {
        "type": "function",
        "function": {
            "name": "read_file",
            "description": "Read the full indexed content of a file by its path.",
            "parameters": {
                "type": "object",
                "properties": {
                    "path": {"type": "string", "description": "Relative file path (e.g. src/main.py)"}
                },
                "required": ["path"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "list_files",
            "description": "List files in the repository matching a glob pattern.",
            "parameters": {
                "type": "object",
                "properties": {
                    "pattern": {"type": "string", "description": "Glob pattern (e.g. **/*.py, src/*.ts)"}
                },
                "required": ["pattern"]
            }
        }
    },
    {
        "type": "function",
        "function": {
            "name": "search",
            "description": "Semantic search over the codebase. Returns the most relevant code chunks.",
            "parameters": {
                "type": "object",
                "properties": {
                    "query": {"type": "string", "description": "What to search for"}
                },
                "required": ["query"]
            }
        }
    }
]

async def _execute_tool(tool_name: str, args: dict, repo_id: str) -> str:
    if tool_name == "read_file":
        return await _read_file(repo_id, args.get("path", ""))
    if tool_name == "list_files":
        return await _list_files(repo_id, args.get("pattern", "*"))
    if tool_name == "search":
        return await _search(repo_id, args.get("query", ""))
    return f"Unknown tool: {tool_name}"


async def _get_openai_key(user_id: str) -> str | None:
    async with AsyncSessionLocal() as db:
        result = await db.execute(
            select(User.encrypted_openai_key).where(User.id == uuid.UUID(user_id))
        )
        encrypted = result.scalar_one_or_none()
        if not encrypted:
            return None
        return decrypt_token(encrypted)


async def create_summary(repo_id: str, user_id: str):
    async with AsyncSessionLocal() as db:
        repo_result = await db.execute(select(Repository).where(Repository.id == uuid.UUID(repo_id)))
        repo = repo_result.scalar_one_or_none()

        if not repo:
            return

        if repo.summary is not None:
            return

        files_result = await db.execute(
            select(RepositoryFile).where(RepositoryFile.repository_id == uuid.UUID(repo_id))
        )
        files = files_result.scalars().all()

    api_key = await _get_openai_key(user_id)
    if not api_key:
        return

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

    messages = [
        {"role": "system", "content": system_prompt},
        {"role": "user", "content": f"Analyze this codebase. Here is the folder structure:\n\n{file_tree}"}
    ]

    client = AsyncOpenAI(api_key=api_key)
    max_iterations = 10
    summary = None

    for i in range(max_iterations):
        if i == max_iterations - 1:
            messages.append({
                "role": "user",
                "content": "You have reached the maximum number of tool calls. Please write the summary now based on what you have gathered so far."
            })

        response = await client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            tools=TOOLS,
            tool_choice="auto"
        )
        choice = response.choices[0]

        if choice.finish_reason == "tool_calls":
            messages.append(choice.message)
            for tool_call in choice.message.tool_calls:
                tool_name = tool_call.function.name
                args = json.loads(tool_call.function.arguments)
                result = await _execute_tool(tool_name, args, repo_id)
                messages.append({
                    "role": "tool",
                    "tool_call_id": tool_call.id,
                    "content": result
                })
        else:
            summary = choice.message.content
            break

    if summary:
        async with AsyncSessionLocal() as db:
            await db.execute(
                text('UPDATE "Repositories" SET "Summary" = :summary, "SummaryGeneratedAt" = :ts WHERE "Id" = :id'),
                {"summary": summary, "ts": datetime.utcnow(), "id": uuid.UUID(repo_id)}
            )
            await db.commit()
        print(f"Summary generated for repo {repo_id}")
    else:
        print(f"Summary agent hit max iterations for repo {repo_id}")


async def _process_entries(r, entries):
    for entry_id, data in entries:
        repo_id_bytes = data.get(b"repoId")
        user_id_bytes = data.get(b"userId")
        if not repo_id_bytes or not user_id_bytes:
            print(f"Missing repoId or userId in entry {entry_id}: {data}")
            await r.xack(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, entry_id)
            continue
        repo_id = repo_id_bytes.decode()
        user_id = user_id_bytes.decode()
        try:
            await create_summary(repo_id, user_id)
            await r.xack(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, entry_id)
        except Exception as e:
            print(f"Exception while processing summary: {e}")


async def start_summary_worker():
    while True:
        try:
            r = await aioredis.from_url(REDIS_URL)
            try:
                await r.xgroup_create(SUMMARY_STREAM_NAME, SUMMARY_GROUP_NAME, id="0", mkstream=True)
            except Exception:
                pass

            print("Summary worker started, listening for jobs")

            pending = await r.xreadgroup(SUMMARY_GROUP_NAME, CONSUMER_NAME, {SUMMARY_STREAM_NAME: "0"}, count=100)
            if pending:
                for _, entries in pending:
                    print(f"Recovering {len(entries)} pending summary jobs")
                    await _process_entries(r, entries)

            while True:
                try:
                    messages = await r.xreadgroup(
                        SUMMARY_GROUP_NAME,
                        CONSUMER_NAME,
                        {SUMMARY_STREAM_NAME: ">"},
                        block=5000,
                        count=1
                    )
                except Exception as e:
                    print(f"xreadgroup error: {e}")
                    break

                if not messages:
                    continue

                for _, entries in messages:
                    await _process_entries(r, entries)

        except Exception as e:
            print(f"Worker connection error: {e}, retrying in 5s")
            await asyncio.sleep(5)
