import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { repoServices } from "../services/repoServices"
import { useMe } from "../../auth/hooks/authHooks"
import { useEffect } from "react"
import { toast } from "sonner"

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

export const useConnectRepo = () => {
    const qc = useQueryClient()
    return useMutation({
        mutationFn:(repoId:string)=>repoServices.connectRepo(repoId),
        onSuccess:(data,repoId) => {
            qc.setQueryData(['files',repoId],data)
            qc.invalidateQueries({queryKey:['indexing-status', repoId]})
        },
    })
}

export const useFiles = (repoId:string) => {
    return useQuery({
        queryKey:['files', repoId],
        queryFn:()=>repoServices.getFiles(repoId),
        enabled: !!repoId,
        staleTime: 60 * 1000 * 5
    })
}

export const useIndexingStatus = (repoId: string) => {
    const query = useQuery({
        queryKey: ['indexing-status', repoId],
        queryFn: () => repoServices.getStatus(repoId),
        enabled: !!repoId,
        staleTime: 0,
    })

    useEffect(() => {
        if (!repoId) return
        const status = query.data?.status
        if (status === 'Completed' || status === 'Failed') return

        const id = setInterval(() => {
            query.refetch()
        }, 2000)

        return () => clearInterval(id)
    }, [repoId, query.data?.status])

    return query
}

export const useAutoIndexRepo = (repoId: string) => {
  const connectRepo = useConnectRepo();

  useEffect(() => {
    connectRepo.mutate(repoId);
  }, [repoId]);

  return {
    data: connectRepo.data,
    isPending: connectRepo.isPending,
    isError: connectRepo.isError,
    indexingStatus: connectRepo.data?.indexingStatus
  };
};

export const useReIndex = () => {
    const qc = useQueryClient()
    return useMutation({
        mutationFn:(repoId:string) => repoServices.reIndex(repoId),
        onSuccess:(_,repoId)=>{
            toast.success("Re-indexing started")
            qc.invalidateQueries({queryKey:['indexing-status', repoId]})
        },
        onError:()=>toast.error("Failed to re-index this repository")
    })
}