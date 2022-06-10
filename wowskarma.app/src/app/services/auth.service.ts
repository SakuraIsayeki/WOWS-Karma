import { Injectable } from "@angular/core";
import { BehaviorSubject, pipe, switchMap } from "rxjs";
import { environment } from "../../environments/environment";
import { AuthModel } from "../models/AuthModel";
import { AppConfigService } from "./app-config.service";
import { AuthService as ApiAuthService } from "./api/services/auth.service";

declare type JwtParsed =
  { [key: string]: string }
  & { "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role": string[] }

@Injectable()
export class AuthService {
  isLoaded$ = new BehaviorSubject<boolean>(false);
  userInfo$ = new BehaviorSubject<AuthModel | null>(null);
  /**
   * Holds the JWT object with information parsed from token.
   */
  jwtObject: any;

  constructor(
    private appConfigService: AppConfigService,
    private apiAuthService: ApiAuthService,
  ) {
    this.load().then(() => {
      /* Nothing done here */
    });
  }

  get isAuthenticated(): boolean {
    return this.userInfo$.value != null;
  }

  async load() {
    const cookieName = environment.cookies.name[this.appConfigService.currentRegion];
    const cookie = getCookie(cookieName);
    let authData: AuthModel | null = null;

    if (cookie) {
      const jwt = parseJwt(cookie);
      console.debug("JWT:", jwt);

      authData = {
        id: parseInt(jwt["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"]),
        username: jwt["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"],
        roles: jwt["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role"],
        expiration: new Date(jwt["exp"]),
      };

      console.log("Loaded Authentication data:", authData);

      // Send a HEAD request to the Auth endpoint on API, to check if the token is still valid.
      // If it is, we can then assert the user is authenticated.
      // If it is not, we can then logout.
      this.apiAuthService.apiAuthHead().subscribe({
        next: () => {
          this.userInfo$.next(authData);
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
        }
      });
    }
     else {
      console.log("No authentication cookie found.");
    }
  }


  public isInRole(role: string) {
    return this.userInfo$.value?.roles?.includes(role) ?? false;
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
