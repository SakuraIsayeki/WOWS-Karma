import {Component, inject, OnInit} from '@angular/core';
import { AppConfigService } from "../../services/app-config.service";

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  private appConfigService: AppConfigService = inject(AppConfigService);
  currentRegion: string = this.appConfigService.currentRegion;
}
