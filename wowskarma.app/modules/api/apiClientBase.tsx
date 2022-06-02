import {URL} from "url";
import {ApiRegion} from "./apiRegion";
import {getJsonConfig} from "../config/jsonConfig";
import {inject} from "tsyringe";

/**
 * Provides a base class for API clients.
 */
export abstract class ApiClientBase {
    host: URL = new URL("https://api.wows-karma.com");

    constructor(@inject("currentRegion") region: ApiRegion) {
        const jsonConfig = getJsonConfig()
            .then(jsonConfig => {
                // Get the host for the given region.
                this.host = new URL(jsonConfig.regions[region]);
            })
            .catch(error => {
                console.error(error);
            });
    }
}