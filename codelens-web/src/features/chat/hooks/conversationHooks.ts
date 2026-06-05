import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { conversationService } from "../services/conversationService"
import { toast } from "sonner"

export const useCreateConversations = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (repoId:string) => conversationService.create(repoId),
        onSuccess:(_,repoId)=>{
            queryClient.invalidateQueries({queryKey:['conversations',repoId]})
        },
        onError:()=>{
            toast.error("unable to create conversation")
        }
    })
}

export const UseConversations = (repoId:string) => {
    return useQuery({
        queryKey:['conversations',repoId],
        queryFn:()=>conversationService.getByRepoId(repoId),
        staleTime:1000*60*5,
        enabled: !!repoId
    })
}