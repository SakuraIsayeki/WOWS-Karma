import { Component, inject } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";
import { environment } from "../../../../environments/environment";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { AsyncPipe, NgIf } from "@angular/common";
import { NgbCollapse } from "@ng-bootstrap/ng-bootstrap";
import { NavAuthComponent } from "./nav-auth/nav-auth.component";
import { NotificationsButtonComponent } from "./notifications-button/notifications-button.component";

@Component({
  standalone: true,
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  imports: [
    RouterLink,
    RouterLinkActive,
    AsyncPipe,
    NgbCollapse,
    NavAuthComponent,
    NotificationsButtonComponent,
    NgIf
  ]
})
export class NavbarComponent {
  public isCollapsed = true;
  environmentName = environment.name;

  authService = inject(AuthService)


}
