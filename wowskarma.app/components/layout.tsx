import type { ReactElement } from "react";
import Navbar from "./navbar";
import Footer from "./footer";


export default function Layout({ children: content }: { children: ReactElement }, pageProps: any) {
    return(
        <>
            <header>
                <Navbar />
            </header>

            <div className="container" style={{ marginTop: "100px !important" }}>
                <div id="psa-header" role="alert" className="mb-5">

                </div>

                <main role="main">
                    {content}
                </main>
            </div>

            <Footer />
        </>
    )
}