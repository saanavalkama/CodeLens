import { createBrowserRouter } from 'react-router-dom'
import RootLayout from '../components/RootLayout'
import LandingPage from '../pages/LandingPage'
import Login from '../features/auth/pages/Login'
import CallbackPage from '../features/auth/pages/CallbackPage'

export const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      {
        path: '/',
        element: <LandingPage />,
      },
      {
        path: '/login',
        element: <Login />,
      },
      {path:'/auth/callback',
        element:<CallbackPage />
      },
      {
        path:'/app',
        element:<p>welcome!</p>
      }

    ],
  },
])
