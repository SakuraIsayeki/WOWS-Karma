/// <reference types="@angular/localize" />

import { enableProdMode, importProvidersFrom, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { environment } from './environments/environment';
import { bootstrapApplication } from "@angular/platform-browser";
import { AppWrapperComponent } from "./app/app-wrapper.component";
import { AppComponent } from "./app/app.component";
import { AuthService } from "./app/services/auth.service";
import { AppConfigService } from "./app/services/app-config.service";
import { AppInsightsService } from "./app/services/app-insights.service";
import { AppInitService } from "./app/services/app-init.service";
import { AppInitGuard } from "./app/services/guards/app-init.guard";
import { AuthGuard } from "./app/services/guards/auth.guard";
import { HTTP_INTERCEPTORS, provideHttpClient, withInterceptors, withInterceptorsFromDi } from "@angular/common/http";
import { AuthenticationInterceptor } from "./app/services/interceptors/authentication.interceptor";
import { ErrorInterceptor } from "./app/services/interceptors/error.interceptor";
import { MARKED_OPTIONS, provideMarkdown } from "ngx-markdown";
import { provideServiceWorker } from "@angular/service-worker";
import { CommonModule } from "@angular/common";
import { provideRouter } from "@angular/router";
import { routes } from "./app/app-routing.module";
import { ApiModule } from "./app/services/api/api.module";

if (environment.name === "production") {
  enableProdMode();
  //console.debug = (...args: any[]) => {};
}

bootstrapApplication(AppWrapperComponent, {
  providers: [
    AuthService,
    AppConfigService,
    AppInsightsService,
    AppInitService,
    AppInitGuard,
    AuthGuard,
    {provide: HTTP_INTERCEPTORS, multi: true, useClass: AuthenticationInterceptor},
    {provide: HTTP_INTERCEPTORS, multi: true, useClass: ErrorInterceptor},
    importProvidersFrom(CommonModule, ApiModule),
    provideRouter(routes),
    provideExperimentalZonelessChangeDetection(),
    provideHttpClient(withInterceptorsFromDi()),

    provideMarkdown({
      markedOptions: {
        provide: MARKED_OPTIONS,
        useValue: {
          gfm: true,
          breaks: true
        }
      }
    }),

    provideServiceWorker("ngsw-worker.js", {
      enabled: environment.name === "production",
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: "registerWhenStable:30000",
    }),
  ]
})
  .catch(err => console.error(err));
