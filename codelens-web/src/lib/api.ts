import axios from "axios";
import {env} from '../config/env'
import { useAuthStore } from "../store/authStore";

export const api = axios.create({
    baseURL:env.apiBaseUrl,
    withCredentials:true
})

api.interceptors.request.use((config =>{
    const token = useAuthStore.getState().accessToken
    if(token){
        config.headers.Authorization = `Bearer ${token}`
    }
    return config
}))