import {json} from "stream/consumers";

/**
 * Provides a configuration object from a JSON file (default to `/appsettings.json`).
 */
export async function getJsonConfig(path: string = "/appsettings.json"): Promise<any> {
    // Fetches the configuration file, and opens it as a stream.
    const configFile = await fetch(path);
    return await configFile.json();
}