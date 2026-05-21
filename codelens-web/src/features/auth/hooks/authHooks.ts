import { useQuery } from "@tanstack/react-query"
import { authService } from "../services/authService"

export const useMe = () => {
    return useQuery({
        queryKey:['me'],
        queryFn:()=>authService.me(),
        staleTime:1000*60*3
    })
}