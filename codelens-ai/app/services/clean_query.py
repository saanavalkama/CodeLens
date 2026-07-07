import re

STOP_WORDS = {"how", "what", "where", "why", "does", "is", "are", "the", "a", "an", "works", "work"}

def clean_query(text: str) -> str:
    words = text.lower().split()
    stripped = [re.sub("\W", "", w) for w in words]
    arr = [w for w in stripped if w not in STOP_WORDS]
    return  " ".join(arr)

