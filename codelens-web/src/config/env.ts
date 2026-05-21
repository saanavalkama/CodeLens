const apiBaseUrl = import.meta.env.VITE_API_URL

interface Env{
    apiBaseUrl: string
}

if(!apiBaseUrl) throw new Error("Could not resolve base url")

export const env:Env = {
    apiBaseUrl
}