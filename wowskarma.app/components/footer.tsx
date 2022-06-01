import type {ReactElement} from "react";
import "../utilities";
import Link from "next/link";

export default function Footer() {
    return (
        <>
            <footer className="footer text-muted mt-5">
                <div className="container">
                    <div className="row justify-content-between flex-grow-1">
                        <div className="col-auto align-self-end">

                            <div>
                                <strong className="me-3">WOWS Karma (@currentRegion)</strong>v@(Startup.DisplayVersion)
                            </div>

                            <div>
                                <ul className="list-inline">
                                    {getFooterLink("/guidelines", "Guidelines")}
                                    {/* API Link goes here */}
                                    {getFooterLink("https://github.com/SakuraIsayeki/WoWS-Karma", "GitHub")}
                                    {getFooterLink("https://discord.gg/PrW9dtzK9K", "Discord")}
                                </ul>
                            </div>
                        </div>

                        <div className="col-auto align-self-end mb-3">
                            Developed by <a href="https://wows-karma.com/player/503379282,Sakura_Isayeki">Sakura Isayeki</a> <br/>
                            Powered by <a target="_blank" href="https://nodsoft.net/">Nodsoft Systems</a>
                        </div>
                    </div>
                </div>
            </footer>
        </>
    )
}

export function getFooterLink(href: string, text: string) {
    return (
        <li className="list-inline-item">
            <Link href={href} target={"_blank"}>{text}</Link>
        </li>
    )
}