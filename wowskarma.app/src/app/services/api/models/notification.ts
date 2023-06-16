import { PlayerPostDto } from "./player-post-dto";
import { PostModActionDto } from "./post-mod-action-dto";

export type Notification = {
    id: string;
    accountId: number;
    type: NotificationType;
    emittedAt: Date;
    acknowledgedAt?: Date;
}

export enum NotificationType {
    unknown,
    other,
    postAdded,
    postEdited,
    postDeleted,
    postModEdited,
    postModDeleted,
    platformBan,

}

export type PostNotification = Notification & { postId: string, post: PlayerPostDto }
export type PostModDeletedNotification = Notification & { modActionId: string, modAction: PostModActionDto }
export type PostModEditedNotification = Notification & { modActionId: string, modAction: PostModActionDto }
export type PlatformBanNotification = Notification & { reason: string, until?: Date }
