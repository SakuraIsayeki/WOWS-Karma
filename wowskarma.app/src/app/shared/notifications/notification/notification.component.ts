import {
  ChangeDetectionStrategy,
  Component,
  EventEmitter,
  input,
  Input,
  Output,
  TemplateRef,
  ViewChild
} from "@angular/core";
import {
  Notification,
  NotificationType,
  PlatformBanNotification,
  PostModDeletedNotification,
  PostModEditedNotification,
  PostNotification
} from "../../../services/api/models/notification";
import {PlayerPostDto} from "../../../services/api/models/player-post-dto";
import {PostModActionDto} from "../../../services/api/models/post-mod-action-dto";
import { NgIf, DatePipe } from "@angular/common";
import { RouterLink } from "@angular/router";
import { BypassHtmlPipe } from "../../../services/pipes/bypass-html.pipe";

@Component({
    selector: "app-notification",
    styleUrls: ["./notification.component.scss"],
    templateUrl: "./notification.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [
        NgIf,
        RouterLink,
        DatePipe,
        BypassHtmlPipe,
    ],
})
export class NotificationComponent {
  notification = input.required<Notification>();
  
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

  constructor() {
  }

  buildNotificationContent(notification: Notification): { title: string, body: string, link?: string, critical?: boolean } {
    switch (notification.type) {
      case NotificationType.postAdded:
        const postNotification = notification as PostNotification;

        return {
          title: "Post added",
          body: `
            <p>A new post has been added by <b>${postNotification.post.author?.username ?? "unknown"}</b> to your profile.</p>

            ${generateTable([
              ...fromPostInfo(postNotification.post, ["title", "content", "createdAt"])
            ]
          )}`,
          link: `/posts/${postNotification.postId}`,
        };

      case NotificationType.postEdited:
        const postEditedNotification = notification as PostNotification;

        return {
          title: "Post edited",
          body: `
            <p>A post by <b>${postEditedNotification.post.author?.username ?? "unknown"}</b> has been edited on your profile.</p>

            ${generateTable([
              ...fromPostInfo(postEditedNotification.post, ["title", "content", "updatedAt"])
            ]
          )}`,
          link: `/posts/${postEditedNotification.postId}`,
        };

      case NotificationType.postDeleted:
        const postDeletedNotification = notification as PostNotification;

        return {
          title: "Post deleted",
          body: `
            <p>A post by <b>${postDeletedNotification.post.author?.username ?? "unknown"}</b> has been deleted from your profile.</p>

            ${generateTable([
              ...fromPostInfo(postDeletedNotification.post, [])
            ]
          )}`,
          link: `/posts/${postDeletedNotification.postId}`,
        };

      case NotificationType.postModEdited:
        const postModEditedNotification = notification as PostModEditedNotification;
        return {
          title: "Post mod edited",
          body: `
            <p>Your post has been edited by our Community Managers, and is locked from further editing.</p>
            ${generateTable([
              ["Post ID", `<code>${postModEditedNotification.modAction.postId}</code>`],
              ...fromModActionInfo(postModEditedNotification.modAction, ["reason"])
            ])
          }`,
          link: `/posts/${postModEditedNotification.modAction.postId}`,
          critical: true,
        };

      case NotificationType.postModDeleted:
        const postModDeletedNotification = notification as PostModDeletedNotification;

        return {
          title: "Post mod deleted",
          body: `
            <p>Your post was removed by our Community Managers, and is no longer visible on the platform.</p>
            ${generateTable([
              ["Post ID", `<code>${postModDeletedNotification.modAction.postId}</code>`],
              ...fromModActionInfo(postModDeletedNotification.modAction, ["reason"])
            ])
          }`,
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

type tableRow = string | [string, string];

function generateTable(rows: tableRow[]): string {
  return `
    <table class="table table-sm table-borderless">
      <tbody>
        ${rows.map(row => {
          if (typeof row === "string") {
            return `<tr>${row}</tr>`;
          } else {
            return `<tr><th scope="row">${row[0]}</th><td style="white-space: pre-wrap">${row[1]}</td></tr>`;
          }
        }).join("")}
      </tbody>
    </table>
  `;
}

function fromPostInfo(post: PlayerPostDto, includeFields: (keyof PlayerPostDto)[]): tableRow[] {
  let rows: tableRow[] = [`<tr><th scope="row">Post ID</th><td><code>${post.id}</code></td></tr>`];

  // Loop through the fields and add them programmatically to the table
  for (const field of includeFields) {
    // Display the field name as the header (from camelCase to Title Case).
    const fieldName = field.replace(/([A-Z])/g, " $1").replace(/^./, str => str.toUpperCase()).toString();

    // ...and the value as the body (formatted accordingly to type).
    const value = post[field];

    if (field === "createdAt" || field === "updatedAt") {
      // Format the date to a more readable format
      rows.push([fieldName, new Date(value as string).toLocaleString()]);
    } else {
      rows.push([fieldName, value?.toLocaleString() ?? "N/A"]);
    }
  }

  return rows;
}

function fromModActionInfo(modAction: PostModActionDto, includeFields: (keyof PostModActionDto)[]): tableRow[] {
  let rows: tableRow[] = [`<tr><th scope="row">Mod Action ID</th><td><code>${modAction.id}</code></td></tr>`];

  // Loop through the fields and add them programmatically to the table
  for (const field of includeFields) {
    // Display the field name as the header (from camelCase to Title Case).
    const fieldName = field.replace(/([A-Z])/g, " $1").replace(/^./, str => str.toUpperCase()).toString();

    // ...and the value as the body (formatted accordingly to type).
    const value = modAction[field];

    rows.push([fieldName, value?.toLocaleString() ?? "N/A"]);
  }

  return rows;
}
