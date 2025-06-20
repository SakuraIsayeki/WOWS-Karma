/// <reference types="@angular/localize" />

import { enableProdMode, importProvidersFrom } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';


import { environment } from './environments/environment';
import { AuthService } from './app/services/auth.service';
import { AppConfigService } from './app/services/app-config.service';
import { AppInsightsService } from './app/services/app-insights.service';
import { AppInitService } from './app/services/app-init.service';
import { AppInitGuard } from './app/services/guards/app-init.guard';
import { AuthGuard } from './app/services/guards/auth.guard';
import { HTTP_INTERCEPTORS, withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { AuthenticationInterceptor } from './app/services/interceptors/authentication.interceptor';
import { ErrorInterceptor } from './app/services/interceptors/error.interceptor';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { AppRoutingModule } from './app/app-routing.module';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { ApiModule } from './app/services/api/api.module';
import { ServiceWorkerModule } from '@angular/service-worker';
import { NgbCollapseModule, NgbPaginationModule, NgbTooltipModule, NgbModule } from '@ng-bootstrap/ng-bootstrap';
import { NgOptimizedImage } from '@angular/common';
import { MarkdownModule, MARKED_OPTIONS } from 'ngx-markdown';
import { AppWrapperComponent } from './app/app-wrapper.component';

if (environment.name === "production") {
  enableProdMode();
  //console.debug = (...args: any[]) => {};
}

bootstrapApplication(AppWrapperComponent, {
  providers: [
    importProvidersFrom(BrowserModule, AppRoutingModule, ReactiveFormsModule, FormsModule, ApiModule, ServiceWorkerModule.register("ngsw-worker.js", {
      // enabled: environment.production,
      // Register the ServiceWorker as soon as the application is stable
      // or after 30 seconds (whichever comes first).
      registrationStrategy: "registerWhenStable:30000",
    }), NgbCollapseModule, NgbPaginationModule, NgbTooltipModule, NgbModule, NgOptimizedImage, MarkdownModule.forRoot({
      markedOptions: {
        provide: MARKED_OPTIONS,
        useValue: {
          gfm: true,
          breaks: true
        }
      }
    })),
    AuthService,
    AppConfigService,
    AppInsightsService,
    AppInitService,
    AppInitGuard,
    AuthGuard,
    {provide: HTTP_INTERCEPTORS, multi: true, useClass: AuthenticationInterceptor},
    {provide: HTTP_INTERCEPTORS, multi: true, useClass: ErrorInterceptor},
    provideHttpClient(withInterceptorsFromDi()),
  ]
})
  .catch(err => console.error(err));
