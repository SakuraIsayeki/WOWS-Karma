import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
  template: ``,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoutComponent {
  constructor(appConfigService: AppConfigService) {
    document.cookie = environment.cookies.name[appConfigService.currentRegion] + "=; Max-Age=-99999999;";
    window.location.href = window.location.origin;
  }
}
