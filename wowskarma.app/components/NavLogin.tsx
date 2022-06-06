import Head from "next/head";
import {container} from "tsyringe";
import {AuthService} from "../modules/api/AuthService";
import Link from "next/link";
import {JwtPayload} from "jsonwebtoken";
import {useEffect, useState} from "react";



export default function NavLogin() {
    const [user, setUser] = useState<JwtPayload | null>(null);

    useEffect(() => {
        const authService = container.resolve(AuthService);
        authService.getCurrentUser().then(user => {
            if (user) {
                setUser(user);
            }
        });
    }, []);

    const authService = container.resolve(AuthService);

    return (
        <>
            {typeof user !== "undefined" && user !== null
                ? <RenderAuthorized {...{user}} />
                : <RenderUnauthorized />}
        </>
    )
}

function RenderAuthorized({user}: { user: JwtPayload }) {
    const claimTypes = require("claimtypes");

    return (
        <>
            <li className="navbar-text text-light">Welcome, {user[claimTypes.name]}</li>

            <li className="nav-item">
                {getNavLink(`player/${user[claimTypes.nameIdentifier]},${user[claimTypes.name]}`, "Profile")}
            </li>
        </>
    );
}

function RenderUnauthorized() {
    return (
        <>
            <li className="nav-item">
                {getNavLink(
                    `/login`,
                    "Login")}
            </li>
        </>
    );
}

function getNavLink(href: string, text: string) {
    return (
        <li key={text} className="nav-item">
            <Link href={href}><a className="nav-link text-light">{text}</a></Link>
        </li>
    )
}