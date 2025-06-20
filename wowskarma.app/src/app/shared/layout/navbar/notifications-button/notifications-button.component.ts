import { ChangeDetectionStrategy, Component } from "@angular/core";
import { BehaviorSubject } from "rxjs";
import { NotificationsMenuComponent } from "../../../notifications/notifications-menu/notifications-menu.component";
import { NgIf, AsyncPipe } from "@angular/common";

@Component({
    selector: "app-notifications-button",
    templateUrl: "./notifications-button.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NotificationsMenuComponent,
        NgIf,
        AsyncPipe,
    ],
})
export class NotificationsButtonComponent {
    count$ = new BehaviorSubject(0);

    constructor() { }
}
