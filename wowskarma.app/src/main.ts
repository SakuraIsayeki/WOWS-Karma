/// <reference types="@angular/localize" />

import { enableProdMode, importProvidersFrom, provideExperimentalZonelessChangeDetection } from '@angular/core';
import { environment } from './environments/environment';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppConfigService } from "./app/services/app-config.service";
import { AppWrapperComponent } from "./app/app-wrapper.component";
import { AuthService } from "./app/services/api/services";
import { AppInsightsService } from "./app/services/app-insights.service";
import { AppInitService } from "./app/services/app-init.service";
import { AppInitGuard } from "./app/services/guards/app-init.guard";
import { AuthGuard } from "./app/services/guards/auth.guard";
import { provideHttpClient, withInterceptors, withInterceptorsFromDi } from "@angular/common/http";
import { authenticationInterceptor } from "./app/services/interceptors/authentication.interceptor";
import { errorInterceptor } from "./app/services/interceptors/error.interceptor";
import { CommonModule } from "@angular/common";
import { ApiModule } from "./app/services/api/api.module";
import { provideRouter } from "@angular/router";
import { MARKED_OPTIONS, provideMarkdown } from 'ngx-markdown';
import { provideServiceWorker } from "@angular/service-worker";
import { routes } from "./app/routes";

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
    importProvidersFrom(CommonModule, ApiModule),
    provideRouter(routes),
    provideExperimentalZonelessChangeDetection(),
    provideHttpClient(
      withInterceptors([
        authenticationInterceptor,
        errorInterceptor
      ]),
      withInterceptorsFromDi()
    ),

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
