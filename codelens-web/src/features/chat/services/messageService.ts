import { api } from "@/lib/api";
import type { Message, MessageResponse } from "@/types/types";

const BASE_URL = "/api/chat"

export interface MessageRequest{
    repoId: string,
    conversationId:string,
    message: string
}

export interface GetMessagesRequest{
    repoId:string, 
    conversationId: string
}

export const messageService = {
    sendMessage: async (data:MessageRequest): Promise<MessageResponse> => {
        const response = await api.post(`${BASE_URL}/${data.repoId}/${data.conversationId}`,{message: data.message})
        return response.data
    },
    
    getMessages: async(data:GetMessagesRequest):Promise<Message[]>=> {
        const response = await api.get(`${BASE_URL}/${data.repoId}/${data.conversationId}`)
        return response.data
    }
}