import { createBrowserRouter, Navigate } from 'react-router-dom'
import RootLayout from '../components/RootLayout'
import LandingPage from '../pages/LandingPage'
import Login from '../features/auth/pages/Login'
import CallbackPage from '../features/auth/pages/CallbackPage'
import AppProtectedRoute from '../features/auth/components/AppProtectedRoute'
import DashBoard from '../pages/DashBoard'
import WorkSpace from '../pages/Workspace'
import SettingsPage from '../features/users/components/SettingsPage'

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
        element:<AppProtectedRoute/>,
        children:[
            {index:true, element:<Navigate to="dashboard" />},
            {path:"dashboard", element:<DashBoard /> },
            {path:"dashboard/:id", element:<WorkSpace />},
            {path:"settings", element:<SettingsPage />}
        ]
      }
    ],
  },
])
