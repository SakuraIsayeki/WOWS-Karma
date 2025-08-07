import { ChangeDetectionStrategy, Component, OnInit, inject } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
    template: ``,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
})
export class LoginComponent implements OnInit {
    private route = inject(ActivatedRoute);
    private appConfigService = inject(AppConfigService);

    /** Inserted by Angular inject() migration for backwards compatibility */
    constructor(...args: unknown[]);


    constructor() {
    }

    ngOnInit(): void {
        this.route.queryParams.subscribe(redirectPath => {
            const redirectUri = new URL(redirectPath["redirectUri"] ?? "", window.location.origin);
            let loginUri = new URL(`/api/auth/login`, environment.apiHost[this.appConfigService.currentRegion]);
            loginUri.searchParams.append("redirectUri", encodeURIComponent(redirectUri.href));
            window.location.href = loginUri.href;
        });
    }
}

