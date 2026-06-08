from app.models.search import SearchRequest
from app.services.embedder import embed_query
from app.services.retrieval import retrieve_chunks
from openai import AsyncOpenAI
import os

class SearchService:
    def __init__(self):
        self._client: AsyncOpenAI | None = None

    def _get_client(self) -> AsyncOpenAI:
        if self._client is None:
            self._client = AsyncOpenAI(api_key=os.getenv("OPENAI_API_KEY"))
        return self._client

    async def search(self, request:SearchRequest) -> str:
        embedded = embed_query(request.query)
        chunks = await retrieve_chunks(request.repoId, embedded, 8)
        messages = self._build_prompt(request.query, chunks, request.history)
        return await self._call_LLM(messages)


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
    
    async def _call_LLM(self, messages:list[dict]) -> str:
        res = await self._get_client().chat.completions.create(
            model="gpt-4o-mini",
            messages=messages,
            temperature=0.1
        )
        return res.choices[0].message.content
    