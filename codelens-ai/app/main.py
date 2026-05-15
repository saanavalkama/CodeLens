from fastapi import FastAPI

app = FastAPI(title ="CodeLens AI service")

@app.get("/health")
def health():
  return {"status": "ok"}