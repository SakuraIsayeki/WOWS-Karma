import { Injectable, inject } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { HubConnectionState } from "@microsoft/signalr";
import { Subject } from "rxjs";
import { environment } from "../../../environments/environment";
import { PlayerPostDto } from "../api/models/player-post-dto";
import { AppConfigService } from "../app-config.service";
import { AuthService } from "../auth.service";
import { HubBase } from "./hub-base";

@Injectable({
    providedIn: "root"
})
export class PostsHub extends HubBase {
    connection = this.buildHubConnection("/api/hubs/post");

    private onNewPost = new Subject<PlayerPostDto>();
    private onEditedPost = new Subject<PlayerPostDto>();
    private onDeletedPost = new Subject<string>();

    newPost$ = this.onNewPost.asObservable();
    editedPost$ = this.onEditedPost.asObservable();
    deletedPost$ = this.onDeletedPost.asObservable();


    constructor() {
        const appConfigService = inject(AppConfigService);
        const authService = inject(AuthService);

        super(appConfigService, authService);

        this.connection.on("newPost", x => this.onNewPost.next(x));
        this.connection.on("editedPost", x => this.onEditedPost.next(x));
        this.connection.on("deletedPost", x => this.onDeletedPost.next(x));

        this.connect().then(() => {
            console.log("Connected to Posts hub.");
        });
    }
}

