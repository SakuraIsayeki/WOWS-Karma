import { Injectable } from '@angular/core';
import {ApiRegion} from "../models/ApiRegion";
import packageJson from 'package.json';

@Injectable({
  providedIn: 'root'
})
/**
 * Provides variable application configuration, supplementing the {@link environment} files.
 * @class AppConfigService
 */
export class AppConfigService {
  /**
   * Current region covered by the application.
   */
  public currentRegion: ApiRegion = this.getApiRegionFromLocation() as ApiRegion;

  /**
   * Current application version.
   */
  public version = packageJson.version;


  constructor() { }

  /**
   * Gets the API region from the subdomain of the specified URL.
   * Uses the {@link REGEX_API_SUBDOMAIN} regex to extract the region.
   * @param url The URL to get the region from.
   * @param defaultRegion The default region to use if the region cannot be extracted.
   * @returns The API region.
   */
  static getApiRegion(url: URL | string, defaultRegion: ApiRegion = ApiRegion.EU): ApiRegion {
    const host : string = url instanceof URL ? url.host : url;

    // Match the host against the regex.
    const match = REGEX_API_SUBDOMAIN.exec(host);

    // Return the region if matched, otherwise the default region.
    return ApiRegion[(match?.groups?.['region'] ?? defaultRegion) as keyof typeof ApiRegion];
  }

  /**
   * Gets the API region from the subdomain of the request URL.
   * Uses the {@link getApiRegion} function to get the region.
   * Uses the browser's location to get the URL.
   * @returns The API region.
   * @see {@link ApiRegion}
   * @see {@link getApiRegion}
   * @see {@link https://developer.mozilla.org/en-US/docs/Web/API/Location}
   * */
  getApiRegionFromLocation(): ApiRegion | undefined {
    // Wait for the browser's location to be available.

    if (typeof window?.location === "undefined" ) {
      // This means we're SSR. We'll have to get the region from the server request URL.
    }

    return AppConfigService.getApiRegion(window.location.host);
  }
}

/**
 * Defines a regular expression for matching the subdomain of wows-karma.com, based on the API region.
 * It also matches the TLD, used by EU region.
 * Regions are defined in the ApiRegion enum.
 * @type {RegExp}
 * @constant
 * @see https://regex101.com/r/XjyrMT/2
 */
const REGEX_API_SUBDOMAIN = /(?:|(?<region>|NA|RU|ASIA)\.)wows-karma\.com(?:\/.*)?$/gim;

