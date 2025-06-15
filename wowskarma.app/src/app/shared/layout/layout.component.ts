import {Component, inject, OnInit} from '@angular/core';
import { AppConfigService } from "../../services/app-config.service";
import { NavbarComponent } from "./navbar/navbar.component";
import { HtmlLoaderComponent } from "../components/html-loader/html-loader.component";
import { RouterOutlet } from "@angular/router";
import { FooterComponent } from "./footer/footer.component";

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  imports: [
    NavbarComponent,
    HtmlLoaderComponent,
    RouterOutlet,
    FooterComponent
  ],
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  private appConfigService: AppConfigService = inject(AppConfigService);
  currentRegion: string = this.appConfigService.currentRegion;
}
