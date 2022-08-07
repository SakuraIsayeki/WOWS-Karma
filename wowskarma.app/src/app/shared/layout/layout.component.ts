import { Component, OnInit } from '@angular/core';
import { AppConfigService } from "../../services/app-config.service";

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  currentRegion: string = this.appConfigService.currentRegion;

  constructor(private appConfigService: AppConfigService) { }
}
