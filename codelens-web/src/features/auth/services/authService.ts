import { api } from "../../../lib/api"
import { env } from "../../../config/env"

export const authService = {
    
   connect: () => {
        window.location.href = `${env.apiBaseUrl}/api/auth/github/login`
    },

    refreshToken:async() => {
        const response = await api.post("/api/auth/refresh")
        return response.data
    }
}