import type { FileNode, RepoFile } from "../../../types/types";

export function buildFileTree(files: RepoFile[]): FileNode[] {
  const root: Record<string, any> = {};

  for (const file of files) {
    const parts = file.path.split("/");
    let current = root;

    for (const part of parts) {
      if (!current[part]) {
        current[part] = { _file: null };
      }
      current = current[part];
    }
    // store the original file on the leaf
    current._file = file;
  }

  function toNodes(obj: Record<string, any>, prefix = ""): FileNode[] {
    return Object.keys(obj)
      .filter((key) => key !== "_file")
      .map((name) => {
        const id = prefix ? `${prefix}/${name}` : name;
        const children = toNodes(obj[name], id);
        return children.length > 0
          ? { id, name, children }
          : { id, name };
      });
  }

  return toNodes(root);
}