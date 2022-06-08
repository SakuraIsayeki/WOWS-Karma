import { HTTP_INTERCEPTORS, HttpClientModule } from "@angular/common/http";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { BrowserModule } from "@angular/platform-browser";
import { AppRoutingModule } from "./app-routing.module";
import { AppComponent } from "./app.component";
import { IndexComponent } from "./pages/index/index.component";
import { NotFoundComponent } from "./pages/not-found/not-found.component";
import { SearchComponent } from "./pages/player/search/search.component";
import { ApiModule } from "./services/api/api.module";
import { AppInitService } from "./services/app-init.service";
import { AppInitGuard } from "./services/guards/app-init.guard";
import { ErrorInterceptor } from "./services/interceptors/error.interceptor";
import { FooterComponent } from "./shared/layout/footer.component";
import { LayoutComponent } from "./shared/layout/layout.component";
import { NavbarComponent } from "./shared/layout/navbar.component";
import { AppWrapperComponent } from './app-wrapper.component';

@NgModule({
  declarations: [
    AppComponent,
    LayoutComponent,
    NavbarComponent,
    FooterComponent,
    IndexComponent,
    NotFoundComponent,
    SearchComponent,
    AppWrapperComponent,
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
    AppInitService,
    AppInitGuard,
    { provide: HTTP_INTERCEPTORS, multi: true, useClass: ErrorInterceptor },
  ],
  bootstrap: [AppWrapperComponent],
})
export class AppModule {
}

