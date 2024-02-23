import { Component, inject } from "@angular/core";
import { AuthService } from "src/app/services/auth.service";
import { environment } from "../../../../environments/environment";

@Component({
  selector: "app-navbar",
  templateUrl: "./navbar.component.html",
  styleUrls: ["./navbar.component.scss"],
})
export class NavbarComponent {
  public isCollapsed = true;
  environmentName = environment.name;

  authService = inject(AuthService)


}
