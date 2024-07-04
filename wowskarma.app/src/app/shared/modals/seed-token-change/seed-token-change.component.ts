import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { environment } from "../../../../environments/environment";
import { AuthService } from "../../../services/api/services/auth.service";
import { AppConfigService } from "../../../services/app-config.service";

@Component({
  standalone: true,
  templateUrl: "./seed-token-change.component.html",
  styleUrls: ["./seed-token-change.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SeedTokenChangeComponent {
  @Input() modal!: NgbModalRef;

  constructor(private apiAuthService: AuthService, private appConfigService: AppConfigService) { }

  static OpenModal(modalService: NgbModal) {
    const modalRef = modalService.open(SeedTokenChangeComponent, {});
    modalRef.componentInstance.modal = modalRef;
  }

  onSubmit() {
    this.apiAuthService.apiAuthRenewSeedPost().subscribe(() => {
      document.cookie = environment.cookies.name[this.appConfigService.currentRegion] + "=; Max-Age=-99999999;";
      window.location.href = window.location.origin;
    });
  }
}
