import { api } from "@/lib/api"
import type { ConversationResponse } from "@/types/types"

const BASE_URL = "/api/conversation"

export const conversationService = {

    create: async(repoId:string):Promise<ConversationResponse>=>{
        const response = await api.post<ConversationResponse>(`${BASE_URL}/${repoId}`)
        return response.data
    },

    getByRepoId: async(repoId:string):Promise<ConversationResponse[]>=>{
        const response = await api.get<ConversationResponse[]>(`${BASE_URL}/${repoId}`)
        return response.data
    }
}