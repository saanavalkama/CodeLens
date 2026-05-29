from tree_sitter_languages import get_language, get_parser
from dataclasses import dataclass

CHUNK_LINES = 40
OVERLAP_LINES = 10

LANGUAGE_MAP = {
    ".py": "python",
    ".ts": "typescript",
    ".tsx": "typescript",
    ".js": "javascript",
    ".jsx": "javascript",
    ".cs": "c_sharp",
}

@dataclass
class Chunk:
    content: str
    start_line: int
    end_line: int
    chunk_index: int

def get_tree_sitter_language(file_path: str):
    ext = "." + file_path.rsplit(".", 1)[-1].lower()
    lang_name = LANGUAGE_MAP.get(ext)
    if not lang_name:
        return None
    try:
        return get_language(lang_name), get_parser(lang_name)
    except Exception:
        return None

def extract_node_chunks(source_code: str, language, parser) -> list[Chunk]:
    """Split at function/class boundaries using AST"""
    tree = parser.parse(bytes(source_code, "utf-8"))
    lines = source_code.splitlines()
    
    # node types that represent meaningful boundaries
    SPLIT_NODES = {
        "function_definition",      # Python
        "class_definition",         # Python
        "function_declaration",     # JS/TS
        "method_definition",        # JS/TS
        "class_declaration",        # JS/TS
        "arrow_function",           # JS/TS
        "method_declaration",       # C#
        "class_declaration",        # C#
        "interface_declaration",    # C#/TS
    }
    
    chunks = []
    chunk_index = 0
    
    def traverse(node):
        nonlocal chunk_index
        if node.type in SPLIT_NODES:
            start = node.start_point[0]
            end = node.end_point[0]
            node_lines = lines[start:end + 1]
            
            # if node fits in one chunk, take it whole
            if len(node_lines) <= CHUNK_LINES:
                chunks.append(Chunk(
                    content="\n".join(node_lines),
                    start_line=start,
                    end_line=end,
                    chunk_index=chunk_index
                ))
                chunk_index += 1
            else:
                # node is too large, fall back to fixed size with overlap
                for fixed_chunk in fixed_size_chunks(
                    "\n".join(node_lines), 
                    start_offset=start
                ):
                    fixed_chunk.chunk_index = chunk_index
                    chunks.append(fixed_chunk)
                    chunk_index += 1
        else:
            for child in node.children:
                traverse(child)
    
    traverse(tree.root_node)
    return chunks

def fixed_size_chunks(content: str, start_offset: int = 0) -> list[Chunk]:
    """Fallback: fixed size with overlap"""
    lines = content.splitlines()
    chunks = []
    chunk_index = 0
    i = 0
    
    while i < len(lines):
        end = min(i + CHUNK_LINES, len(lines))
        chunk_lines = lines[i:end]
        
        chunks.append(Chunk(
            content="\n".join(chunk_lines),
            start_line=start_offset + i,
            end_line=start_offset + end - 1,
            chunk_index=chunk_index
        ))
        
        chunk_index += 1
        i += CHUNK_LINES - OVERLAP_LINES  # slide with overlap
    
    return chunks

def chunk_file(content: str, file_path: str) -> list[Chunk]:
    """Main entry point — tries smart chunking, falls back to fixed size"""
    result = get_tree_sitter_language(file_path)
    
    if result:
        language, parser = result
        try:
            chunks = extract_node_chunks(content, language, parser)
            if chunks:
                return chunks
        except Exception as e:
            print(f"Smart chunking failed for {file_path}: {e}")
    
    # fallback for unsupported languages or parse failures
    return fixed_size_chunks(content)