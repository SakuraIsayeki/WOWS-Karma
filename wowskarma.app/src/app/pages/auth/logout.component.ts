import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
    template: ``,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
})
export class LogoutComponent {
    private appConfigService = inject(AppConfigService);

    /** Inserted by Angular inject() migration for backwards compatibility */
    constructor(...args: unknown[]);

    constructor() {
        document.cookie = environment.cookies.name[this.appConfigService.currentRegion] + "=; Max-Age=-99999999;";
        window.location.href = window.location.origin;
    }
}
