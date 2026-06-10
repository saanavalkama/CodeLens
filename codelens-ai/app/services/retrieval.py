import asyncpg
import os
from dataclasses import dataclass

@dataclass
class RetrievedChunk:
    content: str
    file_path: str
    start_line: int
    end_line: int
    similarity: float

async def retrieve_chunks(
    repo_id: str,
    query_embedding: list[float],
    limit: int = 8
) -> list[RetrievedChunk]:
    conn = await asyncpg.connect(os.getenv("DATABASE_URL"))
    
    try:
        rows = await conn.fetch("""
            SELECT
                fc."Content" as content,
                fc."StartLine" as start_line,
                fc."EndLine" as end_line,
                rf."Path" as file_path,
                1 - (fc."Embedding" <=> $1::vector) AS similarity
            FROM "FileChunks" fc
            JOIN "RepositoryFiles" rf ON fc."RepositoryFileId" = rf."Id"
            WHERE rf."RepositoryId" = $2::uuid
            ORDER BY fc."Embedding" <=> $1::vector
            LIMIT $3
        """, str(query_embedding), repo_id, limit)

        return [
            RetrievedChunk(
                content=row["content"],
                file_path=row["file_path"],
                start_line=row["start_line"],
                end_line=row["end_line"],
                similarity=row["similarity"]
            )
            for row in rows
        ]
    finally:
        await conn.close()