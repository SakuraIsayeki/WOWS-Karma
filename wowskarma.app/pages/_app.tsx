import "../styles/app.scss";
import "bootstrap-icons/font/bootstrap-icons.scss";

import "reflect-metadata";
import type {AppProps} from "next/app";
import type {ApiRegion} from "../modules/api/ApiRegion";
import Layout from "../components/Layout";
import {getApiRegion} from "../modules/config/envConfig";
import {container} from "tsyringe";
import {getApiHost} from "../modules/api/ApiClientBase";
import {SWRConfig} from "swr";
import {GetCurrentRegionAuthCookieName} from "../modules/api/AuthService";
import {ReactNode} from "react";


function App({Component, pageProps}: AppProps) {
    const apiRegion: ApiRegion = getCurrentApiRegion(pageProps);

    container.register<ApiRegion>("currentRegion", {useValue: apiRegion});
    console.debug("Current Region:", apiRegion);

    const host = new URL(getApiHost(apiRegion));
    container.register<URL>("apiHost", {useValue: host});
    console.debug("Using API Host:", host);

    const cookieName = GetCurrentRegionAuthCookieName(apiRegion);
    container.register("authCookieName", {useValue: cookieName});
    console.debug("Using Auth Cookie Name:", cookieName);

    const cookieDomain = process.env.NEXT_PUBLIC_COOKIE_DOMAIN;
    container.register("authCookieDomain", {useValue: cookieDomain});
    console.debug("Using Auth Cookie Domain:", cookieDomain);

    return (
        <>
            <SafeHydrate>
                <Layout {...pageProps}>
                    <SWRConfig value={{ provider: () => new Map() }}>
                        <Component {...pageProps} />
                    </SWRConfig>
                </Layout>
            </SafeHydrate>
        </>
    )
}

export default App

function getCurrentApiRegion(props: { appUrl: string | URL; }): ApiRegion {
    return typeof window === "undefined" ? getApiRegion(props.appUrl) : getApiRegion(window.location.href);
}

function SafeHydrate({ children }: { children: ReactNode }) {
    return (
        <div suppressHydrationWarning>
            {typeof window === 'undefined' ? null : children}
        </div>
    )
}