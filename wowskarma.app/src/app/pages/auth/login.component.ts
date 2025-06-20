import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
    template: ``,
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
})
export class LoginComponent implements OnInit {

    constructor(private route: ActivatedRoute, private appConfigService: AppConfigService) {
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

