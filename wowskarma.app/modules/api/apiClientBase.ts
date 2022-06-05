import {ApiRegion} from "./apiRegion";
import {container, inject} from "tsyringe";
import getConfig from "next/config"
import {getEnvironmentVariable} from "../config/envConfig";
import * as url from "url";

/**
 * Provides a base class for API clients.
 */
export abstract class ApiClientBase {
    /**
     * API Region to use for requests.
     * @type {ApiRegion}
     * @memberof ApiClientBase
     */
    public readonly Region: ApiRegion = container.resolve("currentRegion");

    /**
     * Base URL for API requests.
     * @type {URL}
     * @memberof ApiClientBase
     * @see {@link https://developer.mozilla.org/en-US/docs/Web/API/URL}
     */
    public readonly Host: URL = container.resolve("apiHost");

    /**
     * Base fetcher for API requests.
     * @param url URL to fetch.
     */
    protected static readonly fetcher = (url: URL | RequestInfo) => fetch(url).then(res => res.json());
}

/**
 * Gets the API host for the current region.
 * @param region The region to get the host for.
 * @returns The host for the current region.
 */
export function getApiHost(region: ApiRegion): string {
    switch (region) {
        case ApiRegion.NA:
            return process.env.NEXT_PUBLIC_API_HOST_NA as string;
        case ApiRegion.CIS:
            return process.env.NEXT_PUBLIC_API_HOST_CIS as string;
        case ApiRegion.SEA:
            return process.env.NEXT_PUBLIC_API_HOST_SEA as string;
        default:
            return process.env.NEXT_PUBLIC_API_HOST_EU as string;
    }
}