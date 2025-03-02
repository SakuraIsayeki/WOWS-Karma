import { ChangeDetectionStrategy, Component, inject, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
  template: ``,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LoginComponent implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly appConfigService = inject(AppConfigService);

  ngOnInit(): void {
    this.route.queryParams.subscribe(redirectPath => {
      const redirectUri = new URL(redirectPath["redirectUri"] ?? "", window.location.origin);
      let loginUri = new URL(`/api/auth/login`, environment.apiHost[this.appConfigService.currentRegion]);
      loginUri.searchParams.append("redirectUri", encodeURIComponent(redirectUri.href));
      window.location.href = loginUri.href;
    });
  }
}

