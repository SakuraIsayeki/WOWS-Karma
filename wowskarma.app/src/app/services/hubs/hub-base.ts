import { HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import * as signalR from "@microsoft/signalr";
import { createLogger } from "@microsoft/signalr/dist/esm/Utils";
import { Subject } from "rxjs";
import { environment } from "../../../environments/environment";
import { AppConfigService } from "../app-config.service";
import { AuthService } from "../auth.service";
import { inject } from "@angular/core";

export abstract class HubBase {
    abstract connection: HubConnection;
    protected authService: AuthService = inject(AuthService);
    onConnected$ = new Subject();


    protected constructor(protected appConfigService: AppConfigService) { }

    buildHubConnection(url: URL | string, authenticate = false, builderFunc = HubBase.defaultHubBuilder): HubConnection {
        url = url instanceof URL ? url.href : new URL(url, environment.apiHost[this.appConfigService.currentRegion]).href;

        let hub = new signalR.HubConnectionBuilder()
            .withUrl(url, { withCredentials: authenticate, accessTokenFactory: () => authenticate ? this.authService.authToken$.getValue() ?? "" : "" });

        return builderFunc(hub).build();
    }

    async connect(): Promise<void> {
        if (this.connection) {
            try {
                if (this.connection.state !== HubConnectionState.Disconnected) {
                    await this.connection.stop();
                }
                await this.connection.start();
                this.onConnected$.next(void 0);
            } catch (e) {
                console.error("Failed to connect to hub.", e);
            }
        } else {
            console.error("Hub connection is not initialized.");
        }
    }

    disconnect(): Promise<boolean> {
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

    protected static readonly defaultHubBuilder: (builder: HubConnectionBuilder) => HubConnectionBuilder = builder => builder
        .withAutomaticReconnect()
        // Disabled the MessagePack serializer, due to casing issues.
        //.withHubProtocol(new signalRMsgPack.MessagePackHubProtocol())
        .configureLogging(environment.name !== "development" ? signalR.LogLevel.Warning : signalR.LogLevel.Information)
}
