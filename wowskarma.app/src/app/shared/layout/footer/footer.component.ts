import {Component, inject} from "@angular/core";
import { map } from 'rxjs/operators';
import { AuthService } from 'src/app/services/auth.service';
import { anyTrue, shareReplayRefCount } from 'src/app/shared/rxjs-operators';
import { environment } from "src/environments/environment";
import { ApiRegion } from "src/app/models/ApiRegion";
import { AppConfigService } from "src/app/services/app-config.service";
import { RouterLink } from "@angular/router";
import { NgIf, AsyncPipe } from "@angular/common";

@Component({
    selector: "app-footer",
    templateUrl: "./footer.component.html",
    styleUrls: ["./footer.component.scss"],
    standalone: true,
    imports: [
        RouterLink,
        NgIf,
        AsyncPipe,
    ],
})
export class FooterComponent {

  public appConfig: AppConfigService = inject(AppConfigService);
  public authService: AuthService = inject(AuthService);

  public currentRegion: ApiRegion | undefined = AppConfigService.GetApiRegionFromLocation();
  public currentApiHost: string = environment.apiHost[this.appConfig.currentRegion];

  isAdmin$ = this.authService.userInfo$.pipe(
    map(user => !!(user?.roles && user.roles.includes("admin"))),
    shareReplayRefCount(1)
  )

  isCM$ = this.authService.userInfo$.pipe(
    map(user => !!(user?.roles && user.roles.includes("mod"))),
    shareReplayRefCount(1)
  )

  // Combine the two previous observables into one
  isStaff$ = anyTrue(this.isAdmin$, this.isCM$);
}

