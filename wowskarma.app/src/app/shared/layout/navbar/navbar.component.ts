import { Component, inject } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";
import { environment } from "../../../../environments/environment";
import { NgbCollapse } from "@ng-bootstrap/ng-bootstrap";
import { RouterLink, RouterLinkActive } from "@angular/router";
import { NavAuthComponent } from "./nav-auth/nav-auth.component";
import { NgIf, AsyncPipe } from "@angular/common";
import { NotificationsButtonComponent } from "./notifications-button/notifications-button.component";

@Component({
    selector: "app-navbar",
    templateUrl: "./navbar.component.html",
    styleUrls: ["./navbar.component.scss"],
    standalone: true,
    imports: [
        NgbCollapse,
        RouterLink,
        RouterLinkActive,
        NavAuthComponent,
        NgIf,
        NotificationsButtonComponent,
        AsyncPipe,
    ],
})
export class NavbarComponent {
  public isCollapsed = true;
  environmentName = environment.name;

  authService = inject(AuthService)


}
