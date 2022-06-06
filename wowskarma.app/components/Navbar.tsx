import Link from "next/link";
import NavLogin from "./NavLogin";

export default function Navbar() {
    return(
        <nav
            className="navbar fixed-top navbar-expand-sm navbar-toggleable-sm navbar-dark bg-dark box-shadow mb-5 px-3">
            <div className="container-fluid ps-0">
                <div className="align-content-center mx-3">
                    <Link href="/"><a className='navbar-brand'>WOWS Karma</a></Link>
                </div>

                <div id="navbarNavContent" className="navbar-collapse collapse">
                    <ul className="navbar-nav flex-grow-1">
                        {getNavLinks([
                            { href: "/", text: "Home" },
                            { href: "/player", text: "Players" },
                            { href: "/clan", text: "Clans" },
                            { href: "/posts" , text: "Posts" },
                        ])}
                    </ul>

                    <ul className="navbar-nav flex-grow 0">
                        <NavLogin />

                        <li className="nav-item ml-3 msl-5">
                            <button className="btn btn-link text-light">
                                <i className="bi bi-moon" id="icon-theme-selector"></i>
                            </button>
                        </li>
                    </ul>
                </div>

                <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNavContent">
                    <span className="navbar-toggler-icon"></span>
                </button>
            </div>
        </nav>
    )
}


/*
 * Generates HTML for a given array of links (href, text).
 */
function getNavLinks(links: Array<{href: string, text: string}>) {
    return links.map(link => getNavLink(link.href, link.text));
}

function getNavLink(href: string, text: string) {
    return (
        <li key={text} className="nav-item">
            <Link href={href}><a className="nav-link text-light">{text}</a></Link>
        </li>
    )
}