import {Component, inject, OnInit} from '@angular/core';
import { AppConfigService } from "../../services/app-config.service";
import { RouterOutlet } from "@angular/router";
import { FooterComponent } from "./footer/footer.component";
import { HtmlLoaderComponent } from "../components/html-loader/html-loader.component";
import { NavbarComponent } from "./navbar/navbar.component";

@Component({
  selector: 'app-layout',
  templateUrl: './layout.component.html',
  standalone: true,
  imports: [
    RouterOutlet,
    FooterComponent,
    HtmlLoaderComponent,
    NavbarComponent
  ],
  styleUrls: ['./layout.component.scss']
})
export class LayoutComponent {
  private readonly appConfigService: AppConfigService = inject(AppConfigService);
  protected readonly currentRegion = this.appConfigService.currentRegion;
}
