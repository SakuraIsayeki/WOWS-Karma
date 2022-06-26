import { ChangeDetectionStrategy, Component } from "@angular/core";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
    template: ``,
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoutComponent {
    constructor(private appConfigService: AppConfigService) {
        document.cookie = environment.cookies.name[this.appConfigService.currentRegion] + "=; Max-Age=-99999999;";
        window.location.href = window.location.origin;
    }
}
