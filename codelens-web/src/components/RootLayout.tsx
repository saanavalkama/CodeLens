import { Outlet } from "react-router-dom";
import Header from "./Header";

export default function RootLayout(){
    return(
        <div>
          <Header />
          <main className="pt-14">
            <Outlet />
          </main>
        </div>
    )
}