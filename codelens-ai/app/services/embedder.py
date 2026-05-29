from sentence_transformers import SentenceTransformer
from app.services.chunking import Chunk

model = SentenceTransformer("microsoft/codebert-base")

def embed_chunks(chunks: list[Chunk]) -> list[tuple[Chunk, list[float]]]:
    texts = [chunk.content for chunk in chunks]
    embeddings = model.encode(texts, show_progress_bar=False)
    return list(zip(chunks, embeddings.tolist()))