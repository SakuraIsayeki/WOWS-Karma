import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { AppComponent } from "./app.component";
import { IndexComponent } from "./pages/index/index.component";
import { NotFoundComponent } from "./pages/not-found/not-found.component";
import { ProfileComponent } from "./pages/player/profile/profile.component";
import { SearchComponent as PlayerSearchComponent } from "./pages/player/search/search.component";
import { AppInitGuard } from "./services/guards/app-init.guard";
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
          { path: "player", component: PlayerSearchComponent },
          { path: "player/:idNamePair", component: ProfileComponent },
          { path: "", component: IndexComponent },

          // Last route. Spawn a 404 page.
          { path: "**", component: NotFoundComponent },
        ],
      },
    ]
  }
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {
}
