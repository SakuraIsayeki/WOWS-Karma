import {ChangeDetectionStrategy, Component, inject} from "@angular/core";
import { NavigationEnd, Router, RouterLinkActive, RouterLink } from "@angular/router";
import { shareReplay, startWith } from "rxjs";
import { filter, map } from "rxjs/operators";
import { AuthService } from "src/app/services/auth.service";
import { AsyncPipe } from "@angular/common";

@Component({
    selector: "navbar-auth",
    templateUrl: "./nav-auth.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        RouterLinkActive,
        RouterLink,
        AsyncPipe,
    ],
})
export class NavAuthComponent {
  public authService: AuthService = inject(AuthService);
  private router: Router = inject(Router);

  currentRelativePath$ = this.router.events.pipe(
    filter(e => e instanceof NavigationEnd),
    map(() => this.router.url),
    startWith(this.router.url),
    shareReplay(1)
  );
}
