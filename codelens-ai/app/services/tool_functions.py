import uuid
import fnmatch
from sqlalchemy import select, text
from app.db.session import AsyncSessionLocal
from app.models.models import RepositoryFile, FileChunk
from app.services.embedder import embed_query


async def read_file(repo_id: str, path: str) -> str:
    async with AsyncSessionLocal() as db:
        file_result = await db.execute(
            select(RepositoryFile)
            .where(RepositoryFile.repository_id == uuid.UUID(repo_id))
            .where(RepositoryFile.path == path)
        )
        file = file_result.scalar_one_or_none()

        if not file:
            return f"File not found: {path}"

        chunks_result = await db.execute(
            select(FileChunk)
            .where(FileChunk.repository_file_id == file.id)
            .order_by(FileChunk.chunk_index)
        )
        chunks = chunks_result.scalars().all()

    if not chunks:
        return f"No content indexed for: {path}"

    return "\n".join(chunk.content for chunk in chunks)


async def list_files(repo_id: str, pattern: str) -> str:
    async with AsyncSessionLocal() as db:
        result = await db.execute(
            select(RepositoryFile.path)
            .where(RepositoryFile.repository_id == uuid.UUID(repo_id))
        )
        paths = result.scalars().all()

    matched = [p for p in paths if fnmatch.fnmatch(p, pattern)]

    if not matched:
        return f"No files matched pattern: {pattern}"

    return "\n".join(sorted(matched))


async def search(repo_id: str, query: str) -> str:
    embedding = embed_query(query)
    embedding_str = "[" + ",".join(str(x) for x in embedding) + "]"

    async with AsyncSessionLocal() as db:
        result = await db.execute(
            text("""
                SELECT
                    fc."Content" as content,
                    rf."Path" as file_path,
                    fc."StartLine" as start_line,
                    fc."EndLine" as end_line
                FROM "FileChunks" fc
                JOIN "RepositoryFiles" rf ON fc."RepositoryFileId" = rf."Id"
                WHERE rf."RepositoryId" = :repo_id::uuid
                ORDER BY fc."Embedding" <=> :embedding::vector
                LIMIT 8
            """),
            {"embedding": embedding_str, "repo_id": repo_id}
        )
        rows = result.mappings().all()

    if not rows:
        return "No results found."

    parts = []
    for row in rows:
        parts.append(f"[{row['file_path']}:{row['start_line']}-{row['end_line']}]\n{row['content']}")

    return "\n\n---\n\n".join(parts)
