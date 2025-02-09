import { Component, inject } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";
import { environment } from "../../../../environments/environment";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { NgbCollapse } from "@ng-bootstrap/ng-bootstrap";
import { NotificationsButtonComponent } from "./notifications-button/notifications-button.component";
import { NavAuthComponent } from "./nav-auth/nav-auth.component";
import { AsyncPipe } from "@angular/common";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.scss"],
  imports: [
    RouterLinkActive,
    RouterLink,
    NgbCollapse,
    NotificationsButtonComponent,
    NavAuthComponent,
    AsyncPipe
  ]
})
export class NavbarComponent {
  public isCollapsed = true;
  protected readonly environmentName = environment.name;

  protected readonly authService = inject(AuthService);


}
