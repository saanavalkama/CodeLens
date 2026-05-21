import axios from "axios";
import {env} from '../config/env'
import { useAuthStore } from "../store/authStore";

export const api = axios.create({
    baseURL:env.apiBaseUrl,
    withCredentials:true
})

api.interceptors.request.use((config) => {
    const token = useAuthStore.getState().accessToken
    if(token){
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
})

let refreshing: Promise<string> | null = null

api.interceptors.response.use(
    (response) => response,
    async (error) => {
        const original = error.config
        if (error.response?.status !== 401 || original._retry) {
            return Promise.reject(error)
        }
        original._retry = true

        if (!refreshing) {
            refreshing = api.post("/api/auth/refresh")
                .then(res => {
                    const newToken: string = res.data.accessToken
                    useAuthStore.getState().setToken(newToken)
                    return newToken
                })
                .finally(() => { refreshing = null })
        }

        const newToken = await refreshing
        original.headers.Authorization = `Bearer ${newToken}`
        return api(original)
    }
)