import asyncpg
import os
from dataclasses import dataclass
from services import clean_query

@dataclass
class RetrievedChunk:
    id:str
    content: str
    file_path: str
    start_line: int
    end_line: int
    similarity: float

async def retrieve_chunks_hybrid(
        repo_id:str,
        query_text:str,
        query_embedding:list[float],
        limit: int=8
) -> list[RetrievedChunk]:
    vector_results = await retrieve_chunks(repo_id, query_embedding, limit)
    fts_results = await retrieve_chunks_fts(repo_id, query_text, limit)
    return reciprocal_rank_fusion([vector_results, fts_results])[:8]


async def retrieve_chunks(
    repo_id: str,
    query_embedding: list[float],
    limit: int = 8
) -> list[RetrievedChunk]:
    conn = await asyncpg.connect(os.getenv("DATABASE_URL"))
    
    try:
        rows = await conn.fetch("""
            SELECT
                fc."Id" as id,
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
                similarity=row["similarity"],
                id=str(row["id"])
                
            )
            for row in rows
        ]
    finally:
        await conn.close()

async def retrieve_chunks_fts(
    repo_id: str,
    query_text: str,
    limit: int = 8
) -> list[RetrievedChunk]:
    cleaned = clean_query(query_text)
    terms = cleaned.split()
    
    if not terms:
        return []  # nothing meaningful left to search
    
    tsquery_str = " | ".join(terms)
    
    conn = await asyncpg.connect(os.getenv("DATABASE_URL"))

    try:
        rows = await conn.fetch("""
            SELECT
                fc."Id" as id,
                fc."Content" as content,
                fc."StartLine" as start_line,
                fc."EndLine" as end_line,
                rf."Path" as file_path,
                ts_rank(fc.search_vector, to_tsquery('simple', $1)) AS similarity
            FROM "FileChunks" fc
            JOIN "RepositoryFiles" rf ON fc."RepositoryFileId" = rf."Id"
            WHERE rf."RepositoryId" = $2::uuid
            AND fc.search_vector @@ to_tsquery('simple', $1)
            ORDER BY similarity DESC
            LIMIT $3
        """, tsquery_str, repo_id, limit)

        return [
            RetrievedChunk(
                content=row["content"],
                file_path=row["file_path"],
                start_line=row["start_line"],
                end_line=row["end_line"],
                similarity=row["similarity"],
                id=str(row["id"])
            )
            for row in rows
        ]
    finally:
        await conn.close()

    
RRF_K = 60  

def reciprocal_rank_fusion(
    rankings: list[list[RetrievedChunk]],#vector and fts
    k: int = RRF_K
) -> list[RetrievedChunk]:
    scores: dict[str, float] = {}
    chunks_by_id: dict[str, RetrievedChunk] = {}

    for ranking in rankings:
        for rank, chunk in enumerate(ranking):
            scores[chunk.id] = scores.get(chunk.id, 0.0) + 1.0 / (k + rank + 1)
            chunks_by_id.setdefault(chunk.id, chunk)  # keep first-seen version

    ranked_ids = sorted(scores, key=scores.get, reverse=True)
    return [chunks_by_id[cid] for cid in ranked_ids]