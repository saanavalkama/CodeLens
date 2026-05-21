import { Navigate,Outlet } from "react-router-dom"
import { useMe } from "../hooks/authHooks"

export default function AppProtectedRoute(){

    const {data:me,isPending,isError} = useMe()

    if(isPending) return <div>Loading...</div>
    if(isError) return <div>Something went worng</div>
    if(!me) return <Navigate to="/login" replace />

    return <Outlet />
}