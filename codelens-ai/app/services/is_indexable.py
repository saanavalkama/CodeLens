import os

INDEXABLE_EXTENSIONS = {
    # Python
    ".py", ".pyi", ".pyw",
    
    # JavaScript / TypeScript
    ".js", ".jsx", ".ts", ".tsx", ".mjs", ".cjs",
    
    # C / C++
    ".c", ".h", ".cpp", ".cc", ".cxx", ".hpp", ".hxx", ".hh",
    
    # C#
    ".cs", ".csx",
    
    # Java
    ".java",
    
    # Go
    ".go",
    
    # Rust
    ".rs",
    
    # Ruby
    ".rb", ".rake", ".gemspec",
    
    # PHP
    ".php", ".phtml",
    
    # Swift
    ".swift",
    
    # Kotlin
    ".kt", ".kts",
    
    # Scala
    ".scala", ".sc",
    
    # R
    ".r", ".R",
    
    # Shell
    ".sh", ".bash", ".zsh", ".fish",
    
    # PowerShell
    ".ps1", ".psm1", ".psd1",
    
    # Lua
    ".lua",
    
    # Perl
    ".pl", ".pm",
    
    # Haskell
    ".hs", ".lhs",
    
    # Elixir
    ".ex", ".exs",
    
    # Erlang
    ".erl", ".hrl",
    
    # Clojure
    ".clj", ".cljs", ".cljc",
    
    # F#
    ".fs", ".fsx", ".fsi",
    
    # Dart
    ".dart",
    
    # Julia
    ".jl",
    
    # Nim
    ".nim",
    
    # Zig
    ".zig",
    
    # Web
    ".html", ".htm", ".css", ".scss", ".sass", ".less", ".svelte", ".vue",
    
    # Config / data
    ".json", ".jsonc", ".json5",
    ".yaml", ".yml",
    ".toml",
    ".xml",
    ".ini", ".cfg", ".conf",
    ".env", ".env.example",
    ".editorconfig",
    
    # Docs
    ".md", ".mdx", ".markdown",
    ".txt", ".rst", ".adoc",
    
    # SQL / DB
    ".sql", ".prisma", ".graphql", ".gql",
    
    # Infrastructure
    ".tf", ".tfvars",        # Terraform
    ".dockerfile",           # Dockerfile variants
    ".proto",                # Protocol Buffers
    
    # Build / package
    ".gradle", ".maven",
    ".csproj", ".sln", ".vbproj",
    ".gemfile",
    ".makefile",
}

# also catch files with no extension but known names
INDEXABLE_FILENAMES = {
    "dockerfile",
    "makefile",
    "gemfile",
    "rakefile",
    "procfile",
    "jenkinsfile",
    ".gitignore",
    ".gitattributes",
    ".dockerignore",
    "readme",
    "license",
}

def is_indexable(path:str) -> bool:
    filename = os.path.basename(path).lower()
    ext = os.path.splitext(filename)[1].lower()
    return ext in INDEXABLE_EXTENSIONS or filename in INDEXABLE_FILENAMES

