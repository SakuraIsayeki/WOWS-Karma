import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, CanActivateChild, RouterStateSnapshot, UrlTree } from "@angular/router";
import { combineLatest, first, Observable, of, take } from "rxjs";
import { map } from "rxjs/operators";

@Injectable()
export class AppInitGuard implements CanActivate, CanActivateChild {

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.activate();
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.activate();

    route.routeConfig
  }

  private activate() {

    // combineLatest([
    //
    // ])

    // return this.authService.isLoaded$.pipe(
    //   first(),
    //   map(() => true)
    // );

    return of(true);
  }
}
