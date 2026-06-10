from app.models.search import SearchRequest, SearchResponse, ChunkDto, TokenUsage
from app.services.embedder import embed_query
from app.services.retrieval import retrieve_chunks
from openai import AsyncOpenAI
from sqlalchemy import select
from app.db.session import AsyncSessionLocal
from app.services.decrypt import decrypt_token
from app.models.models import User
import uuid
import os

class SearchService:

    async def _get_openai_key(self, user_id: str) -> str:
        async with AsyncSessionLocal() as db:
            result = await db.execute(
                select(User.encrypted_openai_key).where(User.id == uuid.UUID(user_id))
            )
            encrypted = result.scalar_one_or_none()
            if not encrypted:
                raise ValueError("No OpenAI API key configured for this user")
            return decrypt_token(encrypted)

    async def search(self, request:SearchRequest) -> str:
        api_key = await self._get_openai_key(request.userId)
        client = AsyncOpenAI(api_key=api_key)
        embedded = embed_query(request.query)
        chunks = await retrieve_chunks(request.repoId, embedded, 8)
        messages = self._build_prompt(request.query, chunks, request.history)
        answer, usage =  await self._call_LLM(client,messages)
        return SearchResponse(
            answer=answer,
            chunks=[ChunkDto(
                file_path = c.file_path,
                content=c.content,
                start_line=c.start_line,
                end_line=c.end_line,
                similarity=c.similarity
                
            ) for c in chunks],
            usage=usage
        )


    def _build_prompt(self, query, chunks, history):
        context = "\n\n".join([c.content for c in chunks])
        
        messages = [{"role": "system", "content": f"""You are an expert 
            code assistant helping developers understand a codebase.
            Answer questions accurately and concisely based only on the code context provided.
            If the answer cannot be found in the context, say so clearly.
            Always reference specific file paths and function names when relevant.
            Do not make up code that isn't in the context.
            Context:
            {context}"""}]
        
        # add conversation history
        for msg in history:
            messages.append({"role": msg.role, "content": msg.content})
        
        # add current query
        messages.append({"role": "user", "content": query})
        
        return messages   
    
    async def _call_LLM(self, client: AsyncOpenAI, messages:list[dict]) -> tuple[str,TokenUsage]:
        res = await client.chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            temperature=0.1
        )
        usage = TokenUsage(
            prompt_tokens=res.usage.prompt_tokens,
            completion_tokens=res.usage.completion_tokens,
            total_tokens=res.usage.total_tokens
        )
        return res.choices[0].message.content, usage
    