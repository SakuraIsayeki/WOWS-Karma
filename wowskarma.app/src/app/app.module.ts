import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app-routing.module";
import { AppWrapperComponent } from "./app-wrapper.component";
import { AppComponent } from "./app.component";
import { LoginComponent } from "./pages/auth/login.component";
import { LogoutComponent } from "./pages/auth/logout.component";
import { ProfileComponent as ClanProfileComponent } from "./pages/clan/profile/profile.component";
import { SearchComponent as ClanSearchComponent } from "./pages/clan/search/search.component";
import { IndexComponent } from "./pages/index/index.component";
import { NotFoundComponent } from "./pages/not-found/not-found.component";
import { ProfileComponent as PlayerProfileComponent } from "./pages/player/profile/profile.component";
import { SearchComponent as PlayerSearchComponent } from "./pages/player/search/search.component";
import { PostListComponent } from "./pages/post/list/post-list.component";
import { ViewPostComponent } from "./pages/post/view/view-post.component";
import { ApiModule } from "./services/api/api.module";
import { AppConfigService } from "./services/app-config.service";
import { AppInitService } from "./services/app-init.service";
import { AuthService } from "./services/auth.service";
import { AppInitGuard } from "./services/guards/app-init.guard";
import { AuthGuard } from "./services/guards/auth.guard";
import { AuthenticationInterceptor } from "./services/interceptors/authentication-interceptor.service";
import { ErrorInterceptor } from "./services/interceptors/error.interceptor";
import { ColorHexPipe } from "./services/pipes/colorHex.pipe";
import { FormErrorsPipe } from "./services/pipes/form-errors.pipe";
import { SafeStylePipe } from "./services/pipes/safe-style.pipe";
import { ControlExtensionsDirective } from "./shared/directives/control-extensions.directive";
import { FormErrorsComponent } from "./shared/form-errors/form-errors.component";
import { FooterComponent } from "./shared/layout/footer/footer.component";
import { LayoutComponent } from "./shared/layout/layout.component";
import { NavAuthComponent } from "./shared/layout/navbar/nav-auth/nav-auth.component";
import { NavbarComponent } from "./shared/layout/navbar/navbar.component";
import { PostEditorComponent } from "./shared/modals/create-post/post-editor.component";
import { PostsReceivedComponent } from "./shared/player/profile/posts-received/posts-received.component";
import { PostsSentComponent } from "./shared/player/profile/posts-sent/posts-sent.component";
import { FlairMarkupsComponent } from "./shared/post/flair-markup/flair-markups.component";
import { PostComponent } from "./shared/post/post.component";
import { ChatLogComponent } from "./shared/replay/chat-log/chat-log.component";
import { TeamRosterComponent } from "./shared/replay/team-roster/team-roster.component";
import { SettingsComponent } from './pages/settings/settings.component';

@NgModule({
    declarations: [
        AppComponent,
        LayoutComponent,
        NavbarComponent,
        FooterComponent,
        IndexComponent,
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
        PostListComponent,
        PostEditorComponent,
        ControlExtensionsDirective,
        FormErrorsComponent,
        FormErrorsPipe,
        ClanSearchComponent,
        ClanProfileComponent,
        SettingsComponent,

    ],
    imports: [
        BrowserModule,
        AppRoutingModule,
        ReactiveFormsModule,
        FormsModule,
        HttpClientModule,
        ApiModule,
    ],
    providers: [
        AuthService,
        AppConfigService,
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

