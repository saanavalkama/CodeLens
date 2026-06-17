import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { messageService, type MessageRequest } from "../services/messageService"
import { toast } from "sonner"

export const useMessages = (repoId: string, conversationId: string) => {
    return useQuery({
        queryKey: ['messages', repoId, conversationId],
        queryFn: () => messageService.getMessages({ repoId, conversationId }),
        staleTime: 1000 * 60 * 5,
        enabled: !!repoId && !!conversationId
    })
}

export const useSendMessage = () => {
    const queryClient = useQueryClient()
    return useMutation({
        mutationFn: (data: MessageRequest) => messageService.sendMessage(data),
        onSuccess: (_, variables) => {
            queryClient.invalidateQueries({ queryKey: ['messages', variables.repoId, variables.conversationId] })
            queryClient.invalidateQueries({queryKey:['conversations',variables.repoId]})
        },
        onError: () => {
            toast.error("Failed to send message")
        }
    })
}
