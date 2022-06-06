import {decode, JwtPayload} from 'jsonwebtoken';
import {CookieStorage} from "cookie-storage";
import {ApiClientBase} from "./ApiClientBase";
import {ApiRegion} from "./ApiRegion";
import {container, injectable} from "tsyringe";
import {string} from "prop-types";

/**
 * Defines a service to handle authentication.
 * @class
 * @extends ApiClientBase
 */
@injectable()
export class AuthService extends ApiClientBase {
    private readonly _loginEndpoint = new URL("auth/login", this.Host);
    private readonly _renewSeedEndpoint = new URL("auth/renew-seed", this.Host);
    private readonly _refreshTokenEndpoint = new URL("auth/refresh-token", this.Host);
    private static _cookieStorage = new CookieStorage();

    /**
     * Provides a redirect URL to the login page.
     */
    public get LoginUrl() {
        return this._loginEndpoint;
    }

    /**
     * Gets the current user's information through their token, within the current region's auth cookie.
     * Current region is resolved from DI, using "apiRegion" as the DI key.
     * @returns {JwtPayload | null}
     * @memberof AuthService
     * @method
     * @throws {Error} If the auth cookie is not found.
     * @throws {Error} If the auth cookie is invalid.
     */
    public async getCurrentUser(): Promise<JwtPayload | null> {
        const tokenString = AuthService._cookieStorage.getItem(container.resolve("authCookieName"));


        if (typeof tokenString === "string") {
            const token = decode(tokenString, {json: true});

            if (token === null) {
                throw new Error("Auth cookie invalid.");
            }



            // Check issuer and audience for correct domain
            const domain = container.resolve<string>("AuthCookieName");

            if (token.iss !== domain || token.aud !== domain) {
                throw new Error("Auth cookie invalid.");
            }

            return token;
        }

        return null;
    }
}

/**
 * Gets the specified region's auth cookie name.
 * @returns {string}
 * @method
 */
export function GetCurrentRegionAuthCookieName(region: ApiRegion): string {
    switch (region) {
        case ApiRegion.EU:
            return process.env.NEXT_PUBLIC_COOKIE_NAME_EU as string;
        case ApiRegion.NA:
            return process.env.NEXT_PUBLIC_COOKIE_NAME_NA as string;
        case ApiRegion.CIS:
            return process.env.NEXT_PUBLIC_COOKIE_NAME_CIS as string;
        case ApiRegion.SEA:
            return process.env.NEXT_PUBLIC_COOKIE_NAME_SEA as string;
        default:
            return process.env.NEXT_PUBLIC_COOKIE_NAME_EU as string;
    }
}