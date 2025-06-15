import { Component, inject } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";
import { environment } from "../../../../environments/environment";
import { NgbCollapse } from "@ng-bootstrap/ng-bootstrap";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { NavAuthComponent } from "./nav-auth/nav-auth.component";
import { NotificationsButtonComponent } from "./notifications-button/notifications-button.component";
import { AsyncPipe, NgIf } from "@angular/common";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.scss"],
  imports: [
    NgbCollapse,
    RouterLink,
    RouterLinkActive,
    NavAuthComponent,
    NotificationsButtonComponent,
    NgIf,
    AsyncPipe
  ]
})
export class NavbarComponent {
  public isCollapsed = true;
  environmentName = environment.name;

  authService = inject(AuthService)


}
