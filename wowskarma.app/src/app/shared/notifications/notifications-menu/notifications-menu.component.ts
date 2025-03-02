import { ChangeDetectionStrategy, Component, inject, Output, TemplateRef, viewChild, ViewChild, ViewEncapsulation } from "@angular/core";
import { NgbOffcanvas, NgbOffcanvasRef } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject, combineLatest } from "rxjs";
import { map } from "rxjs/operators";
import { Notification } from "../../../services/api/models/notification";
import { AuthService } from "../../../services/auth.service";
import { sortByDateField } from "../../../services/helpers";
import { NotificationsHub } from "../../../services/hubs/notifications-hub.service";
import { filterNotNull } from "../../rxjs-operators";
import { NotificationComponent } from "../notification/notification.component";
import { CommonModule } from "@angular/common";

@Component({
    selector: "app-notifications-menu",
    templateUrl: "./notifications-menu.component.html",
    styleUrls: ["./notifications-menu.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    encapsulation: ViewEncapsulation.None,
    imports: [
        NotificationComponent,
        CommonModule
    ],
    standalone: true
})
export class NotificationsMenuComponent {
    protected readonly contentTemplate = viewChild<any>("content");

    private menuRef!: NgbOffcanvasRef;

    private notifications: Notification[] = [];
    private _notifications$ = new BehaviorSubject(this.notifications);
    private authService: AuthService = inject(AuthService);
    notifications$ = this._notifications$.asObservable().pipe(
        map(notifications => notifications.sort(sortByDateField<Notification>("emittedAt")))
    );

    @Output() notificationsCount$ = this.notifications$.pipe(
        map(notifications => notifications.length),
    );

    constructor(private notificationsHubService: NotificationsHub, private offCanvasService: NgbOffcanvas) {
        // Fetch pending notifications on init.
        combineLatest([this.authService.userInfo$, notificationsHubService.onConnected$]).pipe(
            filterNotNull(),
        ).subscribe(() => {
            this.notificationsHubService.getPendingNotifications({
                next: ({ item2: n}) => {
                    this.notifications.push(n);
                },
                complete: () => {
                    this._notifications$.next(this.notifications);
                    console.log(this.notifications);
                },
                error: (err) => {
                    console.log(err);
                }
            })
        })
    }

    async clearNotification(id: string) {
        this.notifications = this.notifications.filter(n => n.id !== id);
        this._notifications$.next(this.notifications);
        await this.notificationsHubService.acknowledgeNotifications([id]);
    }

    async clearAllNotifications() {
        const ids = this.notifications.map(n => n.id);
        this.notifications = [];
        this._notifications$.next(this.notifications);
        await this.notificationsHubService.acknowledgeNotifications(ids);
    }

    openMenu() {
        this.menuRef = this.offCanvasService.open(this.contentTemplate, {
            position: "end",
            //panelClass: "bg-body-menu"
            panelClass: "bg-menu-acrylic"
        });
    }

    closeMenu() {
        this.menuRef.close();
    }
}
