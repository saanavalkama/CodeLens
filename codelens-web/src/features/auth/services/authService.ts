import { api } from "../../../lib/api"
import { env } from "../../../config/env"
import { type AuthResponse } from "../../../types/types"

export const authService = {
    
   connect: () => {
        window.location.href = `${env.apiBaseUrl}/api/auth/github/login`
    },

    refreshToken:async() => {
        const response = await api.post("/api/auth/refresh")
        return response.data
    },

    me:async(): Promise<AuthResponse> =>{
        const response = await api.get<AuthResponse>("/api/auth/me")
        return response.data
    },

    logout:async () => {
       await api.post("/api/auth/logout")
    }
}