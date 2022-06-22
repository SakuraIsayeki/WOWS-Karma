import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
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
import { SettingsComponent } from "./pages/settings/settings.component";
import { AppInitGuard } from "./services/guards/app-init.guard";
import { AuthGuard } from "./services/guards/auth.guard";
import { HtmlLoaderComponent } from "./shared/components/html-loader/html-loader.component";
import { LayoutComponent } from "./shared/layout/layout.component";

const routes: Routes = [
    // Set defaults for the app.
    {
        path: "",
        component: AppComponent,
        canActivate: [AppInitGuard],
        children: [
            {
                path: "",
                component: LayoutComponent,

                children: [
                    { path: "", component: HtmlLoaderComponent, data: { path: "/assets/indexcontent.html" } },

                    // Players
                    {
                        path: "player",

                        children: [
                            { path: "", component: PlayerSearchComponent },
                            { path: ":idNamePair", component: PlayerProfileComponent },
                        ],
                    },

                    // Posts
                    {
                        path: "posts",

                        children: [
                            { path: "", component: PostListComponent },
                            { path: ":id", component: ViewPostComponent },
                            { path: "view/:id", redirectTo: ":id" },
                        ],
                    },

                    // Clans
                    {
                        path: "clan",

                        children: [
                            { path: "", component: ClanSearchComponent },
                            { path: ":idNamePair", component: ClanProfileComponent },
                        ],
                    },

                    { path: "login", component: LoginComponent },
                    { path: "logout", component: LogoutComponent },
                    { path: "settings", component: SettingsComponent, canActivate: [AuthGuard] /* data: { roles: ["role1"] } */ },

                    { path: "guidelines", component: HtmlLoaderComponent, data: { path: "/assets/guidelines.html" } },

                    // Last route. Spawn a 404 page.
                    { path: "**", component: NotFoundComponent },
                ],
            },
        ],
    },
];

@NgModule({
    imports: [RouterModule.forRoot(routes)],
    exports: [RouterModule],
})
export class AppRoutingModule {
}
