from sentence_transformers import SentenceTransformer
from app.services.chunking import Chunk

_model = None

def _get_model() -> SentenceTransformer:
    global _model
    if _model is None:
        _model = SentenceTransformer("microsoft/codebert-base")
    return _model

def embed_chunks(chunks: list[Chunk]) -> list[tuple[Chunk, list[float]]]:
    texts = [chunk.content for chunk in chunks]
    embeddings = _get_model().encode(texts, show_progress_bar=False)
    return list(zip(chunks, embeddings.tolist()))