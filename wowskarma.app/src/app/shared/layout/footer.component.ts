import { Component, Inject, OnInit } from "@angular/core";
import { ApiRegion } from "../../models/ApiRegion";
import { AppConfigService } from "../../services/app-config.service";

@Component({
  selector: "app-footer",
  templateUrl: "./footer.component.html",
  styleUrls: ["./footer.component.scss"],
})
export class FooterComponent implements OnInit {

  constructor(
    @Inject(AppConfigService) public appConfig: AppConfigService,
  ) {
  }

  public currentRegion: ApiRegion = AppConfigService.getApiRegionFromLocation() as ApiRegion;

  ngOnInit(): void {
  }

}
