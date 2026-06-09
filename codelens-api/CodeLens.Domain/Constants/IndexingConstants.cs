namespace CodeLens.Domain.Constants;

public static class IndexingConstants
{
    public static readonly HashSet<string> IndexableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".py", ".pyi", ".pyw",
        ".js", ".jsx", ".ts", ".tsx", ".mjs", ".cjs",
        ".c", ".h", ".cpp", ".cc", ".cxx", ".hpp", ".hxx", ".hh",
        ".cs", ".csx",
        ".java", ".go", ".rs",
        ".rb", ".rake", ".gemspec",
        ".php", ".phtml",
        ".swift", ".kt", ".kts",
        ".scala", ".sc",
        ".sh", ".bash", ".zsh", ".fish",
        ".ps1", ".psm1", ".psd1",
        ".lua", ".pl", ".pm",
        ".ex", ".exs", ".erl", ".hrl",
        ".clj", ".cljs", ".cljc",
        ".fs", ".fsx", ".fsi",
        ".dart", ".jl", ".nim", ".zig",
        ".html", ".htm", ".css", ".scss", ".sass", ".less", ".svelte", ".vue",
        ".json", ".jsonc", ".json5",
        ".yaml", ".yml", ".toml", ".xml",
        ".ini", ".cfg", ".conf",
        ".env",
        ".md", ".mdx", ".markdown", ".txt", ".rst",
        ".sql", ".prisma", ".graphql", ".gql",
        ".tf", ".tfvars", ".proto",
        ".csproj", ".sln",
        ".gradle"
    };

    public static readonly HashSet<string> IndexableFilenames = new(StringComparer.OrdinalIgnoreCase)
    {
        "dockerfile", "makefile", "gemfile", "rakefile",
        "procfile", "jenkinsfile", ".gitignore",
        ".gitattributes", ".dockerignore", "readme", "license"
    };

    public static bool IsIndexable(string path)
    {
        var filename = Path.GetFileName(path);
        var ext = Path.GetExtension(path);
        return IndexableExtensions.Contains(ext) ||
               IndexableFilenames.Contains(filename);
    }
}