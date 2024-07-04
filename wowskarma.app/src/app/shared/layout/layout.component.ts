import {Component, inject, OnInit} from '@angular/core';
import { AppConfigService } from "../../services/app-config.service";
import { NavbarComponent } from "./navbar/navbar.component";
import { HtmlLoaderComponent } from "../components/html-loader/html-loader.component";
import { RouterOutlet } from "@angular/router";
import { FooterComponent } from "./footer/footer.component";

@Component({
  standalone: true,
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  styleUrls: ['./layout.component.scss'],
  imports: [
    NavbarComponent,
    HtmlLoaderComponent,
    RouterOutlet,
    FooterComponent

  ]
})
export class LayoutComponent {
  private appConfigService: AppConfigService = inject(AppConfigService);
  currentRegion: string = this.appConfigService.currentRegion;
}
