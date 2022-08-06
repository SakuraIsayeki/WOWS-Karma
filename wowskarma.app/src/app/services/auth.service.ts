import { Injectable } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { map } from "rxjs/operators";
import { environment } from "../../environments/environment";
import { AuthModel } from "../models/AuthModel";
import { UserProfileFlagsDto } from "./api/models/user-profile-flags-dto";
import { AuthService as ApiAuthService } from "./api/services/auth.service";
import { ProfileService } from "./api/services/profile.service";
import { AppConfigService } from "./app-config.service";

declare type JwtParsed =
    { [key: string]: string }
    & { "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": string[] }

@Injectable()
export class AuthService {
    /**
     * Defines if authentication was resolved.
     */
    isLoaded$ = new BehaviorSubject<boolean>(false);
    /**
     * Provides information about the currently authenticated user.
     */
    userInfo$ = new BehaviorSubject<AuthModel | null>(null);
    /**
     * Returns the {@link UserProfileFlagsDto} for the currently logged-in user.
     */
    profileFlags$ = new BehaviorSubject<UserProfileFlagsDto | null>(null);

    /**
     * Provides the raw token, for use outside of the Angular context.
     */
    authToken$ = new BehaviorSubject<string | null>(null);

    constructor(
        private appConfigService: AppConfigService,
        private apiAuthService: ApiAuthService,
        private profileService: ProfileService,
    ) {
        this.load().then(() => {
            /* Nothing done here */
        });
    }

    get isAuthenticated(): boolean {
        return this.userInfo$.value !== null;
    }

    public static IsInRole(user: AuthModel | null, role: string) {
        return user?.roles?.includes(role) ?? false;
    }

    async load() {
        const cookieName = environment.cookies.name[this.appConfigService.currentRegion];
        const cookie = getCookie(cookieName);
        let authData: AuthModel | null = null;

        if (cookie) {
            const jwt = parseJwt(cookie);
            console.debug("JWT:", jwt);
            this.authToken$.next(cookie);

            const date = new Date(Date.UTC(1970, 0, 1)); // UNIX Epoch
            date.setSeconds(parseInt(jwt["exp"]));

            authData = {
                id: parseInt(jwt["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]),
                username: jwt["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
                roles: jwt["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"],
                expiration: date,
            };

            console.log("Loaded Authentication data:", authData);

            // Send a HEAD request to the Auth endpoint on API, to check if the token is still valid.
            // If it is, we can then assert the user is authenticated.
            // If it is not, we can then logout.
            this.apiAuthService.apiAuthHead().subscribe({
                next: () => {
                    this.userInfo$.next(authData);

                    // Get the profile flags for the user.
                    this.profileService.apiProfileIdGet$Json(authData!).subscribe({
                        next: profileFlags => {
                            console.debug("Loaded profile flags:", profileFlags);
                            this.profileFlags$.next(profileFlags);
                        },
                    });

                    this.isLoaded$.next(true);
                },
                error: (error) => {
                    // Authentication failed.
                    if (error.status === 401 || error.status === 403) {
                        this.userInfo$.next(null);
                    }
                    // Something else went wrong.
                    else {
                        console.error(error);
                    }
                    this.isLoaded$.next(true);
                },
            });
        } else {
            console.log("No authentication cookie found.");
            this.isLoaded$.next(true);
        }
    }

    public isInRole(role: string) {
        return this.userInfo$.value?.roles?.includes(role) ?? false;
    }

    public isInRole$(role: string) {
        return this.userInfo$.pipe(
            map(userInfo => userInfo?.roles?.includes(role) ?? false),
        );
    }
}

function parseJwt(token: string): JwtParsed {
    const base64Url = token.split(".")[1];
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(atob(base64).split("").map(function (c) {
        return "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2);
    }).join(""));

    return JSON.parse(jsonPayload);
}

function getCookie(name: string) {
    const value = `; ${document.cookie}`;
    const parts = value.split(`; ${name}=`);
    if (parts.length === 2) {
        return parts.pop()?.split(";").shift();
    }
    return null;
}
