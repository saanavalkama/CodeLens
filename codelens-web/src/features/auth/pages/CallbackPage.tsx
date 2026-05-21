import { useEffect } from "react"
import { useAuthStore } from "../../../store/authStore"
import { useNavigate } from "react-router-dom"
import { authService } from "../services/authService"

export default function CallbackPage(){

    const setToken = useAuthStore(state => state.setToken)
    const navigate = useNavigate()

    useEffect(()=>{
        authService.refreshToken()
        .then(data => {
            setToken(data.accessToken)
            navigate('/app')
        })
        .catch(()=>navigate('/login'))
    },[])

    return(
        <div>
            <p>Logging you in</p>
        </div>
    )
}