/* tslint:disable */
/* eslint-disable */
import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpResponse, HttpContext } from '@angular/common/http';
import { BaseService } from '../base-service';
import { ApiConfiguration } from '../api-configuration';
import { StrictHttpResponse } from '../strict-http-response';
import { RequestBuilder } from '../request-builder';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';

import { AccountFullKarmaDto } from '../models/account-full-karma-dto';
import { AccountListingDto } from '../models/account-listing-dto';
import { PlayerProfileDto } from '../models/player-profile-dto';

@Injectable({
  providedIn: 'root',
})
export class PlayerService extends BaseService {

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {
    const config = inject(ApiConfiguration);
    const http = inject(HttpClient);

    super(config, http);
  }

  /**
   * Path part for operation apiPlayerGet
   */
  static readonly ApiPlayerGetPath = '/api/Player';

  /**
   * Lists all players in the database.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerGet$Plain$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<number>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<number>>;
      })
    );
  }

  /**
   * Lists all players in the database.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerGet$Plain(params?: {
    context?: HttpContext
  }
): Observable<Array<number>> {

    return this.apiPlayerGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<number>>) => r.body as Array<number>)
    );
  }

  /**
   * Lists all players in the database.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerGet$Json$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<number>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<number>>;
      })
    );
  }

  /**
   * Lists all players in the database.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPlayerGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerGet$Json(params?: {
    context?: HttpContext
  }
): Observable<Array<number>> {

    return this.apiPlayerGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<number>>) => r.body as Array<number>)
    );
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
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<AccountListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerSearchQueryGetPath, 'get');
    if (params) {
      rb.path('query', params.query, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
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
    context?: HttpContext
  }
): Observable<Array<AccountListingDto>> {

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
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<AccountListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerSearchQueryGetPath, 'get');
    if (params) {
      rb.path('query', params.query, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
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
    context?: HttpContext
  }
): Observable<Array<AccountListingDto>> {

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
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PlayerProfileDto>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
      rb.query('includeClanInfo', params.includeClanInfo, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerProfileDto>;
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
    context?: HttpContext
  }
): Observable<PlayerProfileDto> {

    return this.apiPlayerIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerProfileDto>) => r.body as PlayerProfileDto)
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
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PlayerProfileDto>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
      rb.query('includeClanInfo', params.includeClanInfo, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerProfileDto>;
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
    context?: HttpContext
  }
): Observable<PlayerProfileDto> {

    return this.apiPlayerIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerProfileDto>) => r.body as PlayerProfileDto)
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<StrictHttpResponse<{
[key: string]: number;
}>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<{
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<StrictHttpResponse<{
[key: string]: number;
}>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<{
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<StrictHttpResponse<Array<AccountFullKarmaDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasFullPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<Array<AccountFullKarmaDto>> {

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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<StrictHttpResponse<Array<AccountFullKarmaDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerKarmasFullPostPath, 'post');
    if (params) {
      rb.body(params.body, 'application/*+json');
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
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
    context?: HttpContext

    /**
     * List of Account IDs
     */
    body?: Array<number>
  }
): Observable<Array<AccountFullKarmaDto>> {

    return this.apiPlayerKarmasFullPost$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<AccountFullKarmaDto>>) => r.body as Array<AccountFullKarmaDto>)
    );
  }

  /**
   * Path part for operation apiPlayerRecalculatePatch
   */
  static readonly ApiPlayerRecalculatePatchPath = '/api/Player/recalculate';

  /**
   * Triggers recalculation of Karma metrics for a given account.
   *
   * Can only be called by Site Administrators.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPlayerRecalculatePatch()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerRecalculatePatch$Response(params?: {

    /**
     * Account ID of player profile
     */
    playerId?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PlayerService.ApiPlayerRecalculatePatchPath, 'patch');
    if (params) {
      rb.query('playerId', params.playerId, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: '*/*',
      context: params?.context
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
   * To access the full response (for headers, for example), `apiPlayerRecalculatePatch$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPlayerRecalculatePatch(params?: {

    /**
     * Account ID of player profile
     */
    playerId?: number;
    context?: HttpContext
  }
): Observable<void> {

    return this.apiPlayerRecalculatePatch$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
