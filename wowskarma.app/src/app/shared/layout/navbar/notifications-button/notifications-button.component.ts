import { ChangeDetectionStrategy, Component } from "@angular/core";
import { BehaviorSubject } from "rxjs";

@Component({
    selector: "app-notifications-button",
    templateUrl: "./notifications-button.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationsButtonComponent {
    count$ = new BehaviorSubject(0);

    constructor() { }
}
