/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { BaseService } from '../base-service';
import { ApiConfiguration } from '../api-configuration';
import { StrictHttpResponse } from '../strict-http-response';
import { RequestBuilder } from '../request-builder';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';

import { AccountFullKarmaDto } from '../models/account-full-karma-dto';
import { AccountListingDto } from '../models/account-listing-dto';
import { PlayerClanProfileDto } from '../models/player-clan-profile-dto';

@Injectable({
  providedIn: 'root',
})
export class PlayerService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiPlayerSearchQueryGet
   */
  static readonly ApiPlayerSearchQueryGetPath = '/api/Player/search/{query}';

  /**
   * Lists accounts containing usernames starting with given search query.
   * (Max. 100 results).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerSearchQueryGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerSearchQueryGet$Plain$Response(params: {

    /**
     * Username search query
     */
    query: string;
  }): Observable<StrictHttpResponse<Array<AccountListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerSearchQueryGetPath, 'get');
    if (params) {
      rb.path('query', params.query, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AccountListingDto>>;
      })
    );
  }

  /**
   * Lists accounts containing usernames starting with given search query.
   * (Max. 100 results).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerSearchQueryGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerSearchQueryGet$Plain(params: {

    /**
     * Username search query
     */
    query: string;
  }): Observable<Array<AccountListingDto>> {

    return this.apiPlayerSearchQueryGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AccountListingDto>>) => r.body as Array<AccountListingDto>)
    );
  }

  /**
   * Lists accounts containing usernames starting with given search query.
   * (Max. 100 results).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerSearchQueryGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerSearchQueryGet$Json$Response(params: {

    /**
     * Username search query
     */
    query: string;
  }): Observable<StrictHttpResponse<Array<AccountListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerSearchQueryGetPath, 'get');
    if (params) {
      rb.path('query', params.query, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AccountListingDto>>;
      })
    );
  }

  /**
   * Lists accounts containing usernames starting with given search query.
   * (Max. 100 results).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerSearchQueryGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerSearchQueryGet$Json(params: {

    /**
     * Username search query
     */
    query: string;
  }): Observable<Array<AccountListingDto>> {

    return this.apiPlayerSearchQueryGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AccountListingDto>>) => r.body as Array<AccountListingDto>)
    );
  }

  /**
   * Path part for operation apiPlayerIdGet
   */
  static readonly ApiPlayerIdGetPath = '/api/Player/{id}';

  /**
   * Fetches the player profile for a given Account ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerIdGet$Plain$Response(params: {

    /**
     * Player account ID
     */
    id: number;

    /**
     * Include clan membership info while fetching player profile.
     */
    includeClanInfo?: boolean;
  }): Observable<StrictHttpResponse<PlayerClanProfileDto>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
      rb.query('includeClanInfo', params.includeClanInfo, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerClanProfileDto>;
      })
    );
  }

  /**
   * Fetches the player profile for a given Account ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerIdGet$Plain(params: {

    /**
     * Player account ID
     */
    id: number;

    /**
     * Include clan membership info while fetching player profile.
     */
    includeClanInfo?: boolean;
  }): Observable<PlayerClanProfileDto> {

    return this.apiPlayerIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerClanProfileDto>) => r.body as PlayerClanProfileDto)
    );
  }

  /**
   * Fetches the player profile for a given Account ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerIdGet$Json$Response(params: {

    /**
     * Player account ID
     */
    id: number;

    /**
     * Include clan membership info while fetching player profile.
     */
    includeClanInfo?: boolean;
  }): Observable<StrictHttpResponse<PlayerClanProfileDto>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
      rb.query('includeClanInfo', params.includeClanInfo, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerClanProfileDto>;
      })
    );
  }

  /**
   * Fetches the player profile for a given Account ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerIdGet$Json(params: {

    /**
     * Player account ID
     */
    id: number;

    /**
     * Include clan membership info while fetching player profile.
     */
    includeClanInfo?: boolean;
  }): Observable<PlayerClanProfileDto> {

    return this.apiPlayerIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerClanProfileDto>) => r.body as PlayerClanProfileDto)
    );
  }

  /**
   * Path part for operation apiPlayerKarmasPost
   */
  static readonly ApiPlayerKarmasPostPath = '/api/Player/karmas';

  /**
   * Fetches Site Karma for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerKarmasPost$Plain()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasPost$Plain$Response(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<StrictHttpResponse<{
[key: string]: number;
}>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<{
        [key: string]: number;
        }>;
      })
    );
  }

  /**
   * Fetches Site Karma for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerKarmasPost$Plain$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasPost$Plain(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<{
[key: string]: number;
}> {

    return this.apiPlayerKarmasPost$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<{
[key: string]: number;
}>) => r.body as {
[key: string]: number;
})
    );
  }

  /**
   * Fetches Site Karma for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerKarmasPost$Json()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasPost$Json$Response(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<StrictHttpResponse<{
[key: string]: number;
}>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<{
        [key: string]: number;
        }>;
      })
    );
  }

  /**
   * Fetches Site Karma for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerKarmasPost$Json$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasPost$Json(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<{
[key: string]: number;
}> {

    return this.apiPlayerKarmasPost$Json$Response(params).pipe(
      map((r: StrictHttpResponse<{
[key: string]: number;
}>) => r.body as {
[key: string]: number;
})
    );
  }

  /**
   * Path part for operation apiPlayerKarmasFullPost
   */
  static readonly ApiPlayerKarmasFullPostPath = '/api/Player/karmas-full';

  /**
   * Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerKarmasFullPost$Plain()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasFullPost$Plain$Response(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<StrictHttpResponse<Array<AccountFullKarmaDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasFullPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AccountFullKarmaDto>>;
      })
    );
  }

  /**
   * Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerKarmasFullPost$Plain$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasFullPost$Plain(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<Array<AccountFullKarmaDto>> {

    return this.apiPlayerKarmasFullPost$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AccountFullKarmaDto>>) => r.body as Array<AccountFullKarmaDto>)
    );
  }

  /**
   * Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerKarmasFullPost$Json()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasFullPost$Json$Response(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<StrictHttpResponse<Array<AccountFullKarmaDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasFullPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<AccountFullKarmaDto>>;
      })
    );
  }

  /**
   * Fetches full Karma metrics (Site Karma and Flairs) for each provided Account ID, where available.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerKarmasFullPost$Json$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPlayerKarmasFullPost$Json(params?: {

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }): Observable<Array<AccountFullKarmaDto>> {

    return this.apiPlayerKarmasFullPost$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AccountFullKarmaDto>>) => r.body as Array<AccountFullKarmaDto>)
    );
  }

  /**
   * Path part for operation apiPlayerRecalculatePost
   */
  static readonly ApiPlayerRecalculatePostPath = '/api/Player/recalculate';

  /**
   * Triggers recalculation of Karma metrics for a given account.
   *
   * Can only be called by Site Administrators.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerRecalculatePost()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerRecalculatePost$Response(params?: {

    /**
     * Account ID of player profile
     */
    playerId?: number;
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerRecalculatePostPath, 'post');
    if (params) {
      rb.query('playerId', params.playerId, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: '*/*'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
      })
    );
  }

  /**
   * Triggers recalculation of Karma metrics for a given account.
   *
   * Can only be called by Site Administrators.
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerRecalculatePost$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerRecalculatePost(params?: {

    /**
     * Account ID of player profile
     */
    playerId?: number;
  }): Observable<void> {

    return this.apiPlayerRecalculatePost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
