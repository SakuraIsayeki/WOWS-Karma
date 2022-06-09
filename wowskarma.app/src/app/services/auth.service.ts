import { Injectable } from "@angular/core";
import { CookieService } from "ngx-cookie-service";
import { BehaviorSubject } from "rxjs";
import { environment } from "../../environments/environment";
import { AuthModel } from "../models/AuthModel";
import { AppConfigService } from "./app-config.service";

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
    private cookieService: CookieService,
  ) {
    this.load().then(() => { /* Nothing done here */ });
  }

  get isAuthenticated() {
    return this.jwtObject != null;
  }

  async load() {
    const cookieName = environment.cookies.name[this.appConfigService.currentRegion]
    const cookie = getCookie(cookieName);
    let authData;

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
    }
    else {
      console.log("No authentication cookie found.");
    }


    console.debug(authData);

    this.jwtObject = null; // TODO: Assign from token
    this.userInfo$.next(authData ?? null);
    this.isLoaded$.next(true);
    // this.userInfo$.value
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
