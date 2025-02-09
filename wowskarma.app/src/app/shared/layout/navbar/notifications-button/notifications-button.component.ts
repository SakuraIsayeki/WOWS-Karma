import { CommonModule } from "@angular/common";
import { ChangeDetectionStrategy, Component } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { NotificationsMenuComponent } from "src/app/shared/notifications/notifications-menu/notifications-menu.component";

@Component({
    selector: "app-notifications-button",
    templateUrl: "./notifications-button.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        NotificationsMenuComponent,
        CommonModule
    ]
})
export class NotificationsButtonComponent {
    count$ = new BehaviorSubject(0);

    constructor() { }
}
