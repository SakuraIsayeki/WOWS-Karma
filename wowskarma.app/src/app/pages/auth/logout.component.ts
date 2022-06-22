import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../../services/app-config.service";

@Component({
  template: ``,
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LogoutComponent {
  constructor(private appConfigService: AppConfigService, private ngbModal: NgbModal) {
    document.cookie = environment.cookies.name[this.appConfigService.currentRegion] + "=; Max-Age=-99999999;";
    window.location.href = window.location.origin;
  }
}
