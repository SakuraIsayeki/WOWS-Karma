import { Component, Inject, OnInit } from "@angular/core";
import { environment } from "../../../../environments/environment";
import { ApiRegion } from "../../../models/ApiRegion";
import { AppConfigService } from "../../../services/app-config.service";

@Component({
  selector: "app-footer",
  templateUrl: "./footer.component.html",
  styleUrls: ["./footer.component.scss"],
})
export class FooterComponent {

  public currentRegion: ApiRegion | undefined = AppConfigService.GetApiRegionFromLocation();
  public currentApiHost: string = environment.apiHost[this.appConfig.currentRegion];

  constructor(
    @Inject(AppConfigService) public appConfig: AppConfigService,
  ) {
  }
}
