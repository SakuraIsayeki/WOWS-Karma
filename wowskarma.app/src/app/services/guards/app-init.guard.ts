import { inject, Injectable, Injector } from "@angular/core";
import { ActivatedRouteSnapshot, RouterStateSnapshot, UrlTree } from "@angular/router";
import { combineLatest, first, Observable, of, take } from "rxjs";
import { map } from "rxjs/operators";
import { AuthService } from "../auth.service";

@Injectable({providedIn: "root"})
export class AppInitGuard  {
  private authService: AuthService = inject(AuthService);

  canActivateChild(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.activate();
  }

  canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
    return this.activate();
  }

  private activate() {

     return this.authService.isLoaded$.pipe(
       first(),
       map(() => true)
     );
  }
}
