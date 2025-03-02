import { CommonModule } from "@angular/common";
import { ChangeDetectionStrategy, Component, model, viewChild } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { NotificationsMenuComponent } from "src/app/shared/notifications/notifications-menu/notifications-menu.component";

@Component({
  selector: "app-notifications-button",
  templateUrl: "./notifications-button.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    NotificationsMenuComponent,
    CommonModule
  ]
})
export class NotificationsButtonComponent {
  protected readonly count = model(0);

  // #menu component is in a @defer block, so it will not be accessible.
  protected readonly menu = viewChild(NotificationsMenuComponent);
}
