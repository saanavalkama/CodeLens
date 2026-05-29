export interface AuthResponse{
    id:string
    githubUsername:string
    userTier: string
}

export interface RepoResponse{
    id: string
    name:string
}

export interface RepoFile{
    path:string,
    type:string
}

export interface ConnectResponse{
    repoId:string,
    indexingStatus:string,
    files: RepoFile[]
}

export interface FileNode {
  id: string;
  name: string;
  children?: FileNode[];
}