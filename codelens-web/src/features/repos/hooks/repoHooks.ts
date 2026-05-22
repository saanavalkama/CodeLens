import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { repoServices } from "../services/repoServices"
import { useMe } from "../../auth/hooks/authHooks"

export const useRepos = () => {
    const {data:me} = useMe()
    return useQuery({
        queryKey:['repos', me?.id],
        queryFn:()=>repoServices.getRepos(),
        staleTime:60*1000*5,
        enabled:!!me
    })
}

export const useFetchRepos = () => {
    const {data:me} = useMe()
    const qc = useQueryClient()
    return useMutation({
        mutationFn:()=>repoServices.fetchRepos(),
        onSuccess:()=>{
            qc.invalidateQueries({queryKey:['repos',me?.id]})
        }
    })
}