import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output, TemplateRef, ViewChild } from "@angular/core";
import { Notification, NotificationType, PlatformBanNotification, PostModDeletedNotification, PostNotification } from "../../../services/api/models/notification";

@Component({
    selector: "app-notification",
    styleUrls: ["./notification.component.scss"],
    templateUrl: "./notification.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class NotificationComponent {
    @Input() notification!: Notification;
    notificationContent!: { title: string, body: string, link?: string, critical?: boolean };

    @Output() onClick = new EventEmitter();
    @Output() onDismiss = new EventEmitter();

    _onClick(e: Event) {
        e.stopPropagation()
        this.onClick.emit();
    }

    _onDismiss(e: Event) {
        e.stopPropagation();
        e.preventDefault();
        this.onDismiss.emit();
    }

    constructor() {}

    buildNotificationContent(notification: Notification): { title: string, body: string, link?: string, critical?: boolean } {
        switch (notification.type) {
            case NotificationType.postAdded:
                const postNotification = notification as PostNotification;
                return {
                    title: "Post added",
                    body: `
                        <p>A new post has been added by <b>${postNotification.post.author?.username ?? "unknown"}</b> to your profile.</p>
                        <p>Post ID: ${postNotification.postId}</p>`,
                    link: `/posts/${postNotification.postId}`,
                };

            case NotificationType.postEdited:
                const postEditedNotification = notification as PostNotification;
                return {
                    title: "Post edited",
                    body: `
                        <p>A post by <b>${postEditedNotification.post.author?.username ?? "unknown"}</b> has been edited on your profile.</p>
                        <p>Post ID: ${postEditedNotification.postId}</p>`,
                    link: `/posts/${postEditedNotification.postId}`,
                };

            case NotificationType.postDeleted:
                const postDeletedNotification = notification as PostNotification;
                return {
                    title: "Post deleted",
                    body: `
                        <p>A post by <b>${postDeletedNotification.post.author?.username ?? "unknown"}</b> has been deleted from your profile.</p>
                        <p>Post ID: ${postDeletedNotification.postId}</p>`,
                    link: `/posts/${postDeletedNotification.postId}`,
                };

            case NotificationType.postModDeleted:
                const postModDeletedNotification = notification as PostModDeletedNotification;
                return {
                    title: "Post mod deleted",
                    body: `
                        <p>Your post was removed by our Community Managers, and is no longer visible on the platform.</p>
                        <p>Post ID: ${postModDeletedNotification.modAction.postId}</p>`,
                    link: `/posts/${postModDeletedNotification.modAction.postId}`,
                    critical: true,
                };

            case NotificationType.platformBan:
                const banNotification = notification as PlatformBanNotification;
                return {
                    title: "Platform ban",
                    body: `
                        <p>You have been banned from the platform ${banNotification.until ? `until ${banNotification.until.toLocaleDateString()}` : "permanently"}.</p>
                        <p>Reason: ${banNotification.reason}</p>`,
                    critical: true,
                };

            default:
                return {
                    title: "Unknown notification",
                    body: "",
                };
        }
    }
}
