import { useParams } from "react-router-dom"
import { useFilesWithAutoConnect } from "../features/repos/hooks/repoHooks"
import FileStructure from "../features/repos/components/FileStructure"

export default function WorkSpace(){

    const params = useParams()
    if(!params.id) return <p>something went wrong</p>
    const {data, isPending, isError, isConnecting, isConnectError } = useFilesWithAutoConnect(params.id)

    if(isPending) return <div>Loading</div>
    if(isConnecting) return <div>Fetching repository tree</div>
    if(isError) return <div>Something went wrong while fetching repos</div>
    if(isConnectError) return <div>Something went wrong while fetching repo tree</div>
    console.log(data)
    return(
        <div>
            <FileStructure files={data.files}/>
        </div>
    )
}