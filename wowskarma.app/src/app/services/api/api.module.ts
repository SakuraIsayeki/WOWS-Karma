/* tslint:disable */
/* eslint-disable */
import { NgModule, ModuleWithProviders, SkipSelf, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiConfiguration, ApiConfigurationParams } from './api-configuration';

import { AuthService } from './services/auth.service';
import { ClanService } from './services/clan.service';
import { ModActionService } from './services/mod-action.service';
import { PlatformBansService } from './services/platform-bans.service';
import { PlayerService } from './services/player.service';
import { PostService } from './services/post.service';
import { ProfileService } from './services/profile.service';
import { ReplayService } from './services/replay.service';
import { StatusService } from './services/status.service';

/**
 * Module that provides all services and configuration.
 */
@NgModule({
  imports: [],
  exports: [],
  declarations: [],
  providers: [
    AuthService,
    ClanService,
    ModActionService,
    PlatformBansService,
    PlayerService,
    PostService,
    ProfileService,
    ReplayService,
    StatusService,
    ApiConfiguration
  ],
})
export class ApiModule {
  static forRoot(params: ApiConfigurationParams): ModuleWithProviders<ApiModule> {
    return {
      ngModule: ApiModule,
      providers: [
        {
          provide: ApiConfiguration,
          useValue: params
        }
      ]
    }
  }

  constructor( 
    @Optional() @SkipSelf() parentModule: ApiModule,
    @Optional() http: HttpClient
  ) {
    if (parentModule) {
      throw new Error('ApiModule is already loaded. Import in your base AppModule only.');
    }
    if (!http) {
      throw new Error('You need to import the HttpClientModule in your AppModule! \n' +
      'See also https://github.com/angular/angular/issues/20575');
    }
  }
}
