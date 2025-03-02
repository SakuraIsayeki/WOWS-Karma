import { ChangeDetectionStrategy, Component, computed, inject, input, Input } from '@angular/core';
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { AppConfigService } from "../../../services/app-config.service";
import { ApiRegion } from "../../../models/ApiRegion";

@Component({
  selector: 'app-cs-ticket-id',
  standalone: true,
  imports: [],
  templateUrl: './cs-ticket-id-help.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CsTicketIdHelpComponent {
  modal = input.required<NgbModalRef>();

  appConfig = inject(AppConfigService);

  region = computed(() => this.appConfig.currentRegion);

  csLinks = computed<[string, string]>(() => {
    /*
     * In order:
     * - Gameplay / Collusions
     * - Chat Issues
     *
     * See: https://github.com/SakuraIsayeki/WOWS-Karma/issues/165
     */

    switch (this.region()) {
      case ApiRegion.EU:
        return [
          'https://eu.wargaming.net/support/en/products/wows/help/29948/29949/29955/29957/',
          'https://eu.wargaming.net/support/en/products/wows/help/29948/29949/29951/29952/'
        ];

      case ApiRegion.NA:
        return [
          'https://na.wargaming.net/support/en/products/wows/help/31336/31337/31338/31339/',
          'https://na.wargaming.net/support/en/products/wows/help/31336/31337/31345/'
        ];

      case ApiRegion.SEA:
        return [
          "https://asia.wargaming.net/support/en/products/wows/help/28687/28688/28689/",
          "https://asia.wargaming.net/support/en/products/wows/help/28687/28688/28694/"
        ];

      default:
        return ['', ''];
    }
  })

  constructor() {
  }

  static OpenModal(modalService: NgbModal) {
    const modalRef = modalService.open(CsTicketIdHelpComponent, { size: "lg" });
    modalRef.componentInstance.modal = modalRef;
  }
}
