import { Injectable } from "@angular/core";
import { Subject } from "rxjs";
import { PlayerPostDto } from "../api/models/player-post-dto";
import { AppConfigService } from "../app-config.service";
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



    constructor(appConfigService: AppConfigService) {
        super(appConfigService);

        this.connection.on("newPost", x => this.onNewPost.next(x));
        this.connection.on("editedPost", x => this.onEditedPost.next(x));
        this.connection.on("deletedPost", x => this.onDeletedPost.next(x));

        this.connect().then(() => {
            console.log("Connected to Posts hub.");
        });
    }
}

