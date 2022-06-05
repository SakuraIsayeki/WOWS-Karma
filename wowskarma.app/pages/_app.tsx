import "../styles/app.scss";
import "bootstrap-icons/font/bootstrap-icons.scss";

import "reflect-metadata";
import type {AppProps} from "next/app";
import type {ApiRegion} from "../modules/api/apiRegion";
import Layout from "../components/layout";
import {getApiRegion} from "../modules/config/envConfig";
import {container} from "tsyringe";
import {getApiHost} from "../modules/api/apiClientBase";


function App({Component, pageProps}: AppProps) {
    container.register<ApiRegion>("currentRegion", {useValue: getCurrentApiRegion(pageProps)});
    container.register<URL>("apiHost", {useValue: new URL(getApiHost(container.resolve("currentRegion")))});

    const host = container.resolve("apiHost");

    console.debug("Using API Host:", host);

    return (
        <Layout {...pageProps}>
            <Component {...pageProps} />
        </Layout>
    )
}

export default App

function getCurrentApiRegion(props: { appUrl: string | URL; }) {
    return typeof window === "undefined" ? getApiRegion(props.appUrl) : getApiRegion(window.location.href);
}
