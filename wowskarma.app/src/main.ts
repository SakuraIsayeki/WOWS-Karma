/// <reference types="@angular/localize" />

import { enableProdMode, importProvidersFrom } from '@angular/core';
import { environment } from './environments/environment';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { authenticationInterceptor } from './app/services/interceptors/authentication.interceptor';
import { errorInterceptor } from './app/services/interceptors/error.interceptor';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { provideServiceWorker } from '@angular/service-worker';
import { MARKED_OPTIONS, provideMarkdown } from 'ngx-markdown';
import { provideRouter } from "@angular/router";
import { routes } from "./app/routes";
import { AppComponent } from "./app/app.component";
import { AppWrapperComponent } from "./app/app-wrapper.component";

if (environment.name === "production") {
  enableProdMode();
  //console.debug = (...args: any[]) => {};
}

bootstrapApplication(AppWrapperComponent, {
  providers: [
    provideRouter(routes),
    importProvidersFrom(BrowserModule),
    provideHttpClient(
      withInterceptors([
        authenticationInterceptor,
        errorInterceptor
      ])
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
      // enabled: environment.production,
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: "registerWhenStable:30000"
    }),
  ]
})
  .catch(err => console.error(err));
