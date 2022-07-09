import { Injectable } from "@angular/core";
import * as signalR from "@microsoft/signalr";
import { HubConnectionState } from "@microsoft/signalr";
import { Subject } from "rxjs";
import { environment } from "../../../environments/environment";
import { PlayerPostDto } from "../api/models/player-post-dto";
import { AppConfigService } from "../app-config.service";

@Injectable({
    providedIn: "root"
})
export class PostsHub {

    private onNewPost = new Subject<PlayerPostDto>();
    private onEditedPost = new Subject<PlayerPostDto>();
    private onDeletedPost = new Subject<string>();

    newPost$ = this.onNewPost.asObservable();
    editedPost$ = this.onEditedPost.asObservable();
    deletedPost$ = this.onDeletedPost.asObservable();

    connection = new signalR.HubConnectionBuilder()
        .withUrl(new URL("/api/hubs/post", environment.apiHost[this.appConfigService.currentRegion]).href)
        .build();

    constructor(private appConfigService: AppConfigService) {
        this.connection.on("newPost", x => this.onNewPost.next(x));
        this.connection.on("editedPost", x => this.onEditedPost.next(x));
        this.connection.on("deletedPost", x => this.onDeletedPost.next(x));

        this.connect().then(() => {
            console.log("Connected to hub.");
        });
    }


    async connect(): Promise<any> {
        if (this.connection) {
            try {
                if (this.connection.state !== HubConnectionState.Disconnected) {
                    await this.connection.stop();
                }
                await this.connection.start();
            } catch (e) {
                console.error("Failed to connect to hub.", e);
            }
        } else {
            console.error("Hub connection is not initialized.");
        }
    }

    disconnect(): Promise<any> {
        return new Promise((resolve) => {
            if (this.connection) {
                try {
                    this.connection.stop()
                        .then(() => {
                            resolve(true);
                        })
                        .catch(() => {
                            console.error("Failed to disconnect from hub.");
                            resolve(false);
                        });
                } catch (e) { }
            } else {
                console.error("Hub connection is not initialized.");
                resolve(false);
            }
        });
    }
}

