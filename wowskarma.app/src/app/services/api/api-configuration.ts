/* tslint:disable */
/* eslint-disable */
import {inject, Inject, Injectable} from '@angular/core';
import { environment } from 'src/environments/environment';
import {AppConfigService} from "../app-config.service";

/**
 * Global configuration
 */
@Injectable({
  providedIn: 'root',
})
export class ApiConfiguration {
  private appConfig: AppConfigService = inject(AppConfigService);
  rootUrl: string = environment.apiHost[this.appConfig.currentRegion];
}

/**
 * Parameters for `ApiModule.forRoot()`
 */
export interface ApiConfigurationParams {
  rootUrl?: string;
}
