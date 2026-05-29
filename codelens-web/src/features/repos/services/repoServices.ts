import { api } from "../../../lib/api";
import type { ConnectResponse, RepoResponse } from "../../../types/types";

const baseUrl = `/api/github`

export const repoServices = {

    getRepos: async():Promise<RepoResponse[]> => {
        const response = await api.get<RepoResponse[]>(`${baseUrl}/repos`)
        return response.data
    },

    fetchRepos:async():Promise<RepoResponse[]> => {
        const response = await api.post<RepoResponse[]>(`${baseUrl}/repos/sync`)
        return response.data
    },

    connectRepo:async(repoId:string):Promise<ConnectResponse> => {
        const response = await api.post<ConnectResponse>(`${baseUrl}/repos/${repoId}/index`)
        return response.data
    },

    getFiles: async(repoId:string):Promise<ConnectResponse> => {
        const response = await api.get<ConnectResponse>(`${baseUrl}/repos/${repoId}/files`)
        return response.data
    }


}