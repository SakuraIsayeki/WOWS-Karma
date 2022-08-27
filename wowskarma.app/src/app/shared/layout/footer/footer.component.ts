import { Component } from "@angular/core";
import { map } from 'rxjs/operators';
import { AuthService } from 'src/app/services/auth.service';
import { anyTrue, shareReplayRefCount } from 'src/app/shared/rxjs-operators';
import { environment } from "src/environments/environment";
import { ApiRegion } from "src/app/models/ApiRegion";
import { AppConfigService } from "src/app/services/app-config.service";

@Component({
  selector: "app-footer",
  templateUrl: "./footer.component.html",
  styleUrls: ["./footer.component.scss"],
})
export class FooterComponent {


  public currentRegion: ApiRegion | undefined = AppConfigService.GetApiRegionFromLocation();
  public currentApiHost: string = environment.apiHost[this.appConfig.currentRegion];

  constructor(public appConfig: AppConfigService, public authService: AuthService) {
  }

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

