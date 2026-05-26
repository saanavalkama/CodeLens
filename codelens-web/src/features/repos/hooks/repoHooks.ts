import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query"
import { repoServices } from "../services/repoServices"
import { useMe } from "../../auth/hooks/authHooks"
import { useEffect } from "react"

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

export const useFilesWithAutoConnect = (repoId: string) => {
  const filesQuery = useFiles(repoId);
  const connectRepo = useConnectRepo();

  useEffect(() => {
    if (
      filesQuery.status === "success" &&
      filesQuery.data?.files.length === 0 &&
      connectRepo.status === "idle"
    ) {
      connectRepo.mutate(repoId);
    }
  }, [filesQuery.status, filesQuery.data, repoId]);

  return {
    ...filesQuery,
    isConnecting: connectRepo.status === "pending",
    isConnectError: connectRepo.status === "error"
  };
};