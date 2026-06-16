import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { userService } from "../services/userService"
import { toast } from "sonner"
import { useMe } from "@/features/auth/hooks/authHooks"


export const useCreateOrUpdateKey = () => {
    const qc = useQueryClient()
    const {data:me} = useMe()
    return useMutation({
        mutationFn:(key :string) => userService.createOrUpdateKey(key),
        onSuccess:()=>{
            toast.success("Key added succesfully")
            qc.invalidateQueries({queryKey:['openai-key-status',me?.id]})

        },
        onError:()=>{
            toast.error("Something went wrong while adding the key")
        }
    })
}

export const useKeyStatus = () => {
    const {data:me} = useMe()
    return useQuery({
        queryKey:['openai-key-status',me?.id],
        queryFn:()=>userService.getKeyStatus(),
        enabled: !!me
    })
}

export const useDeleteUser = () => {
    
    return useMutation({
        mutationFn:()=>userService.deleteUser(),
        onSuccess: () => {
            toast.success("user deleted")
        },
        onError:()=>toast.error("Deletion failed")
    })
}

export const useDeleteKey = () => {
    const {data:me} = useMe()
    const qc = useQueryClient()
    return useMutation({
        mutationFn:()=>userService.deleteKey(),
        onSuccess:()=>{
            toast.success("Deleted the openai key")
            qc.invalidateQueries({queryKey:['openai-key-status',me?.id]})
        },
        onError:()=>toast.error("Openai key deletion failed")
    })
}