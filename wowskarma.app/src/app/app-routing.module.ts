import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { IndexComponent } from './pages/index/index.component';
import { LayoutComponent } from "./shared/layout/layout.component";
import {NotFoundComponent} from "./pages/not-found/not-found.component";

const routes: Routes = [
  // Set defaults for the app.
  {
    path: '',
    component: LayoutComponent,
    children: [
      { path: '', component: IndexComponent, pathMatch: 'full' },

      // Last route. Spawn a 404 page.
      { path: '**', component: NotFoundComponent }
    ]
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
