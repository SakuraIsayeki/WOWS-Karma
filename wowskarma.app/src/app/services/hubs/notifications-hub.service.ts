import { resolve } from "@angular/compiler-cli";
import { Injectable, inject } from "@angular/core";
import { IStreamSubscriber } from "@microsoft/signalr";
import { Subject } from "rxjs";
import { Notification } from "../api/models/notification";
import { AppConfigService } from "../app-config.service";
import { AuthService } from "../auth.service";
import { HubTupleResult } from "../helpers";
import { HubBase } from "./hub-base";

@Injectable({
    providedIn: "root",
})
export class NotificationsHub extends HubBase {
    connection = this.buildHubConnection("/api/hubs/notifications", true);

    private onNewNotification = new Subject<[string, Notification]>();
    private onDeletedNotification = new Subject<string>();

    onNewNotification$ = this.onNewNotification.asObservable();
    onDeletedNotification$ = this.onDeletedNotification.asObservable();

    
    constructor() {
        const appConfigService = inject(AppConfigService);
        const authService = inject(AuthService);

        super(appConfigService, authService);
        this.connect().then(() => {
            console.log("Connected to Notifications hub.");
        });

        this.connection.on("newNotification", x => this.onNewNotification.next(x));
        this.connection.on("deletedNotification", x => this.onDeletedNotification.next(x));
    }


    getPendingNotifications(subscriber: IStreamSubscriber<HubTupleResult<string, Notification>>) {
        return this.connection.stream<HubTupleResult<string, Notification>>("getPendingNotifications").subscribe(subscriber);
    }

    acknowledgeNotifications(ids: string[]) {
        return this.connection.send("acknowledgeNotifications", ids);
    }
}
