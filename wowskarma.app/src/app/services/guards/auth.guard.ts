import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, RouterStateSnapshot, UrlTree } from "@angular/router";
import { Observable, of, switchMap } from "rxjs";
import { filter } from "rxjs/operators";
import { AuthService } from "../auth.service";

@Injectable()
export class AuthGuard implements CanActivate, CanActivateChild {

  constructor(private authService: AuthService) {
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.activate(route, state);
  }

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean | UrlTree> | Promise<boolean | UrlTree> | boolean | UrlTree {
    return this.activate(route, state);
  }

  private activate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot):
    boolean | UrlTree | Observable<boolean | UrlTree> | Promise<boolean | UrlTree> {
    const roles = (route.routeConfig?.data as any)?.roles as (string[] | undefined);
    return this.authService.isLoaded$
      .pipe(
        filter(v => v),
        switchMap(v => {
          if (this.authService.isAuthenticated) {
            if (roles && roles.length > 0) {
              if (roles.some((r: string) => this.authService.isInRole(r))) {
                return of(true);
                // } else if (route.data?.auth?.redirectToNoAccess && this.authConfig.noAccessUrl) {
                //   return of(this.router.parseUrl(this.authConfig.noAccessUrl));
                //} else {
              } else {
                return of(false); // Nothing happens.
              }
            }
            return of(true); // Authenticated.
          }
          return of(false);

          // } else if (route.data?.auth?.redirectToLogin) {
          //  return of(this.router.parseUrl(this.authConfig.loginUrl + '?returnUrl=' + state.url));
          //} else {
          //  return of(this.router.parseUrl('/'));
          //}
        }),
      );
  }
}
