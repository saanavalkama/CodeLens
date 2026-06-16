import { api } from "@/lib/api"

const baseUrl = "/api/user"

export const userService = {

    createOrUpdateKey: async(key:string) => {
        const response = await api.put(`${baseUrl}/openai-key`,{key})
        return response.data

    },

    getKeyStatus: async() => {
        const response = await api.get(`${baseUrl}/openai-key`)
        return response.data
    },
    
    deleteUser: async() => {
        await api.delete(baseUrl)
    }
}