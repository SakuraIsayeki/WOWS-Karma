/// <reference types="@angular/localize" />

import { enableProdMode, importProvidersFrom } from '@angular/core';
import { environment } from './environments/environment';
import { bootstrapApplication, BrowserModule } from "@angular/platform-browser";
import { AppComponent } from "./app/app.component";
import { provideHttpClient, withFetch, withInterceptors } from "@angular/common/http";
import { authenticationInterceptor } from "./app/services/interceptors/authentication.interceptor";
import { errorInterceptor } from "./app/services/interceptors/error.interceptor";
import { provideRouter } from "@angular/router";
import { routes } from "./app/routes";
import { provideServiceWorker } from "@angular/service-worker";
import { MARKED_OPTIONS, provideMarkdown } from "ngx-markdown";

if (environment.name === "production") {
  enableProdMode();
  //console.debug = (...args: any[]) => {};
}

bootstrapApplication(AppComponent, {
  providers: [
    importProvidersFrom(BrowserModule),
    provideRouter(routes),
    provideMarkdown({
      markedOptions: {
        provide: MARKED_OPTIONS,
        useValue: {
          gfm: true,
          breaks: true
        }
      }
    }),
    provideHttpClient(
      withFetch(),
      withInterceptors([
        authenticationInterceptor,
        errorInterceptor
      ])
    ),
    provideServiceWorker("ngsw-worker.js", {
      // enabled: environment.production,
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: "registerWhenStable:30000"
    })
  ],
}).catch(err => console.error(err));
