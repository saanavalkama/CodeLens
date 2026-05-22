import { api } from "../../../lib/api";
import type { RepoResponse } from "../../../types/types";

const baseUrl = `/api/github`

export const repoServices = {

    getRepos: async():Promise<RepoResponse[]> => {
        const response = await api.get<RepoResponse[]>(`${baseUrl}/repos`)
        return response.data
    },

    fetchRepos:async():Promise<RepoResponse[]> => {
        const response = await api.post<RepoResponse[]>(`${baseUrl}/repos/sync`)
        return response.data
    }
}