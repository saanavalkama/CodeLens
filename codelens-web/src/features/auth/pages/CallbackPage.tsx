import { useEffect, useRef } from "react"
import { useAuthStore } from "../../../store/authStore"
import { useNavigate } from "react-router-dom"
import { authService } from "../services/authService"

export default function CallbackPage(){

    const setToken = useAuthStore(state => state.setToken)
    const navigate = useNavigate()
    const called = useRef(false)

    useEffect(()=>{
        if (called.current) return
        called.current = true

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