import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { IndexComponent } from "./pages/index/index.component";
import { NotFoundComponent } from "./pages/not-found/not-found.component";
import { SearchComponent as PlayerSearchComponent } from "./pages/player/search/search.component";
import { LayoutComponent } from "./shared/layout/layout.component";

const routes: Routes = [
  // Set defaults for the app.
  {
    path: "",
    component: LayoutComponent,

    children: [
      { path: "player", component: PlayerSearchComponent },
      { path: "", component: IndexComponent },

      // Last route. Spawn a 404 page.
      { path: "**", component: NotFoundComponent },
    ],
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {
}
