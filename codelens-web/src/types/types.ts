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

export interface ConversationResponse{
   id: string,
   userId:string,
   repoId:string, 
   title:string,
   cratedAt:string 
}

export interface Chunk {
    filePath:string,
    content:string,
    startLine:number,
    endLine:number,
    similarity:number
}

export interface TokenUsage{
    promptTokens:string,
    completionTokens:string,
    totalTokens:string
}

export interface MessageResponse{
    answer:string,
    chunks:Chunk[],
    usage:TokenUsage
}

export interface Message{
    role:string,
    content:string
}

export interface RepoStatus {
    id: string
    status: string
    totalFiles: number
    indexedFiles: number
    percentComplete: number
}