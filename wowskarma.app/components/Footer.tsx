import Link from "next/link";
import {version as appVersion} from "../version";
import {container} from "tsyringe";
import {ApiRegion} from "../modules/api/apiRegion";

const Footer = function () {
    return (
        <footer className="footer text-muted mt-5">
            <div className="container">
                <div className="row justify-content-between flex-grow-1">
                    <div className="col-auto align-self-end">
                        <div id="app-id">
                            <strong className="me-3">WOWS Karma ({container.resolve<ApiRegion>("currentRegion")})</strong>
                            v{appVersion}
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
                        Developed by <a href="https://wows-karma.com/player/503379282,Sakura_Isayeki">Sakura Isayeki</a>
                        <br/>
                        Powered by <a target="_blank" href="https://nodsoft.net/" rel="noreferrer">Nodsoft Systems</a>
                    </div>
                </div>
            </div>
        </footer>
    )
};
export default Footer

function getFooterLink(href: string, text: string) {
    return (
        <li className="list-inline-item">
            <Link href={href} target={"_blank"}>{text}</Link>
        </li>
    )
}
