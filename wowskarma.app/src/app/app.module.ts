import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { ServiceWorkerModule } from "@angular/service-worker";
import { NgbCollapseModule, NgbPaginationModule } from '@ng-bootstrap/ng-bootstrap';
import { AppRoutingModule } from "./app-routing.module";
import { AppWrapperComponent } from "./app-wrapper.component";
import { AppComponent } from "./app.component";
import { LoginComponent } from "./pages/auth/login.component";
import { LogoutComponent } from "./pages/auth/logout.component";
import { ProfileComponent as ClanProfileComponent } from "./pages/clan/profile/profile.component";
import { SearchComponent as ClanSearchComponent } from "./pages/clan/search/search.component";
import { NotFoundComponent } from "./pages/fallbacks/not-found/not-found.component";
import { ProfileComponent as PlayerProfileComponent } from "./pages/player/profile/profile.component";
import { SearchComponent as PlayerSearchComponent } from "./pages/player/search/search.component";
import { ListLatestComponent } from "src/app/pages/post/list/list-latest.component";
import { ViewPostComponent } from "./pages/post/view/view-post.component";
import { SettingsComponent } from "./pages/settings/settings.component";
import { ApiModule } from "./services/api/api.module";
import { AppConfigService } from "./services/app-config.service";
import { AppInitService } from "./services/app-init.service";
import { AppInsightsService } from "./services/app-insights.service";
import { AuthService } from "./services/auth.service";
import { AppInitGuard } from "./services/guards/app-init.guard";
import { AuthGuard } from "./services/guards/auth.guard";
import { AuthenticationInterceptor } from "./services/interceptors/authentication.interceptor";
import { ErrorInterceptor } from "./services/interceptors/error.interceptor";
import { BypassHtmlPipe } from "./services/pipes/bypass-html.pipe";
import { ColorHexPipe } from "./services/pipes/colorHex.pipe";
import { FormErrorsPipe } from "./services/pipes/form-errors.pipe";
import { KarmaColorPipe } from "./services/pipes/karma-color.pipe";
import { PostBorderColorPipe } from "./services/pipes/post-border-color.pipe";
import { SafeStylePipe } from "./services/pipes/safe-style.pipe";
import { WowsNumbersClanLinkPipe } from "./services/pipes/wows-numbers-clan-link.pipe";
import { WowsNumbersPlayerLinkPipe } from "./services/pipes/wows-numbers-player-link.pipe";
import { HtmlLoaderComponent } from "./shared/components/html-loader/html-loader.component";
import { MinMaxMetricComponent } from "./shared/components/minmax-metric/min-max-metric.component";
import { ControlExtensionsDirective } from "./shared/directives/control-extensions.directive";
import { FormErrorsComponent } from "./shared/form-errors/form-errors.component";
import { FooterComponent } from "./shared/layout/footer/footer.component";
import { LayoutComponent } from "./shared/layout/layout.component";
import { NavAuthComponent } from "./shared/layout/navbar/nav-auth/nav-auth.component";
import { NavbarComponent } from "./shared/layout/navbar/navbar.component";
import { NotificationsButtonComponent } from "./shared/layout/navbar/notifications-button/notifications-button.component";
import { PostDeleteComponent } from "./shared/modals/post-delete/post-delete.component";
import { PostEditorComponent } from "./shared/modals/post-editor/post-editor.component";
import { PostModDeleteComponent } from "./shared/modals/post-mod-delete/post-mod-delete.component";
import { SeedTokenChangeComponent } from "./shared/modals/seed-token-change/seed-token-change.component";
import { NotificationComponent } from "./shared/notifications/notification/notification.component";
import { NotificationsMenuComponent } from "./shared/notifications/notifications-menu/notifications-menu.component";
import { PostsReceivedComponent } from "./shared/player/profile/posts-received/posts-received.component";
import { PostsSentComponent } from "./shared/player/profile/posts-sent/posts-sent.component";
import { FlairMarkupsComponent } from "./shared/post/flair-markup/flair-markups.component";
import { PostComponent } from "./shared/post/post.component";
import { ChatLogComponent } from "./shared/replay/chat-log/chat-log.component";
import { TeamRosterComponent } from "./shared/replay/team-roster/team-roster.component";
import { ChatMessageChannelPipe } from './services/pipes/chat-message-channel.pipe';
import { PostModEditorComponent } from 'src/app/shared/modals/post-mod-edit/post-mod-editor.component';
import { UnauthorizedComponent } from './pages/fallbacks/unauthorized/unauthorized.component';
import { ForbiddenComponent } from './pages/fallbacks/forbidden/forbidden.component';
import { ClanRankComponent } from './shared/components/icons/clan-rank/clan-rank.component';
import { PlayerNamelinkComponent } from './shared/components/player-namelink/player-namelink.component';
import { ProfileModActionsViewComponent } from './shared/modals/profile-mod-actions-view/profile-mod-actions-view.component';
import { ProfilePlatformBansViewComponent } from './shared/modals/profile-platform-bans-view/profile-platform-bans-view.component';

@NgModule({
    declarations: [
        AppComponent,
        LayoutComponent,
        NavbarComponent,
        FooterComponent,
        NotFoundComponent,
        PlayerSearchComponent,
        AppWrapperComponent,
        PlayerProfileComponent,
        SafeStylePipe,
        ColorHexPipe,
        PostComponent,
        FlairMarkupsComponent,
        PostsReceivedComponent,
        PostsSentComponent,
        ViewPostComponent,
        TeamRosterComponent,
        ChatLogComponent,
        NavAuthComponent,
        LoginComponent,
        LogoutComponent,
        ListLatestComponent,
        PostEditorComponent,
        ControlExtensionsDirective,
        FormErrorsComponent,
        FormErrorsPipe,
        ClanSearchComponent,
        ClanProfileComponent,
        SettingsComponent,
        SeedTokenChangeComponent,
        PostDeleteComponent,
        PostModDeleteComponent,
        KarmaColorPipe,
        BypassHtmlPipe,
        HtmlLoaderComponent,
        MinMaxMetricComponent,
        PostBorderColorPipe,
        WowsNumbersPlayerLinkPipe,
        WowsNumbersClanLinkPipe,
        NotificationsButtonComponent,
        NotificationsMenuComponent,
        NotificationComponent,
        ChatMessageChannelPipe,
        PostModEditorComponent,
        UnauthorizedComponent,
        ForbiddenComponent,
        ClanRankComponent,
        PlayerNamelinkComponent,
        ProfileModActionsViewComponent,
        ProfilePlatformBansViewComponent,
    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        ReactiveFormsModule,
        FormsModule,
        HttpClientModule,
        ApiModule,
        ServiceWorkerModule.register("ngsw-worker.js", {
            // enabled: environment.production,
            // Register the ServiceWorker as soon as the application is stable
            // or after 30 seconds (whichever comes first).
            registrationStrategy: "registerWhenStable:30000",
        }),
        NgbCollapseModule,
        NgbPaginationModule,
    ],
    providers: [
        AuthService,
        AppConfigService,
        AppInsightsService,
        AppInitService,
        AppInitGuard,
        AuthGuard,
        { provide: HTTP_INTERCEPTORS, multi: true, useClass: AuthenticationInterceptor },
        { provide: HTTP_INTERCEPTORS, multi: true, useClass: ErrorInterceptor },
    ],
    bootstrap: [AppWrapperComponent],
})
export class AppModule {

}

