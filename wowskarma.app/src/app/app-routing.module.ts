import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AppComponent } from "./app.component";
import { LoginComponent } from "./pages/auth/login.component";
import { LogoutComponent } from "./pages/auth/logout.component";
import { IndexComponent } from "./pages/index/index.component";
import { NotFoundComponent } from "./pages/not-found/not-found.component";
import { ProfileComponent } from "./pages/player/profile/profile.component";
import { SearchComponent as PlayerSearchComponent } from "./pages/player/search/search.component";
import { PostListComponent } from "./pages/post/list/post-list.component";
import { ViewPostComponent } from "./pages/post/view/view-post.component";
import { AppInitGuard } from "./services/guards/app-init.guard";
import { AuthGuard } from "./services/guards/auth.guard";
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
          // Players
          {
            path: "player", children: [
              { path: "", component: PlayerSearchComponent },
              { path: ":idNamePair", component: ProfileComponent },
            ],
          },

          // Posts
          {
            path: "posts", children: [
              { path: "", component: PostListComponent },
              { path: ":id", component: ViewPostComponent },
              { path: "view/:id", redirectTo: ":id" },
            ],
          },

          { path: "login", component: LoginComponent },
          { path: "logout", component: LogoutComponent },
          { path: "settings", component: ProfileComponent, canActivate: [AuthGuard] /* data: { roles: ["role1"] } */ },
          { path: "", component: IndexComponent },

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
