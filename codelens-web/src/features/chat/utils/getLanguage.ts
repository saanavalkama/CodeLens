const extMap: Record<string, string> = {
  ts: "typescript",
  tsx: "tsx",
  js: "javascript",
  jsx: "jsx",
  py: "python",
  cs: "csharp",
  java: "java",
  go: "go",
  rs: "rust",
  cpp: "cpp",
  c: "c",
  h: "c",
  rb: "ruby",
  php: "php",
  swift: "swift",
  kt: "kotlin",
  md: "markdown",
  json: "json",
  yaml: "yaml",
  yml: "yaml",
  toml: "toml",
  xml: "xml",
  html: "html",
  css: "css",
  scss: "scss",
  sh: "bash",
  bash: "bash",
  sql: "sql",
}

export function getLanguage(filePath: string | null | undefined): string {
  if (!filePath) return "text"
  const ext = filePath.split(".").pop()?.toLowerCase() ?? ""
  return extMap[ext] ?? "text"
}
