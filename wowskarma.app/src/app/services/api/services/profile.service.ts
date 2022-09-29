/* tslint:disable */
/* eslint-disable */
import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse, HttpContext } from '@angular/common/http';
import { BaseService } from '../base-service';
import { ApiConfiguration } from '../api-configuration';
import { StrictHttpResponse } from '../strict-http-response';
import { RequestBuilder } from '../request-builder';
import { Observable } from 'rxjs';
import { map, filter } from 'rxjs/operators';

import { UserProfileFlagsDto } from '../models/user-profile-flags-dto';

@Injectable({
  providedIn: 'root',
})
export class ProfileService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiProfileIdGet
   */
  static readonly ApiProfileIdGetPath = '/api/Profile/{id}';

  /**
   * Fetches a player's profile flags for given ID.
   * This includes Platform Bans, and Opt-out statuses.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiProfileIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiProfileIdGet$Plain$Response(params: {

    /**
     * Player ID to fetch profile flags from.
     */
    id: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<UserProfileFlagsDto>> {

    const rb = new RequestBuilder(this.rootUrl, ProfileService.ApiProfileIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<UserProfileFlagsDto>;
      })
    );
  }

  /**
   * Fetches a player's profile flags for given ID.
   * This includes Platform Bans, and Opt-out statuses.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiProfileIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiProfileIdGet$Plain(params: {

    /**
     * Player ID to fetch profile flags from.
     */
    id: number;
    context?: HttpContext
  }
): Observable<UserProfileFlagsDto> {

    return this.apiProfileIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<UserProfileFlagsDto>) => r.body as UserProfileFlagsDto)
    );
  }

  /**
   * Fetches a player's profile flags for given ID.
   * This includes Platform Bans, and Opt-out statuses.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiProfileIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiProfileIdGet$Json$Response(params: {

    /**
     * Player ID to fetch profile flags from.
     */
    id: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<UserProfileFlagsDto>> {

    const rb = new RequestBuilder(this.rootUrl, ProfileService.ApiProfileIdGetPath, 'get');
    if (params) {
      rb.path('id', params.id, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<UserProfileFlagsDto>;
      })
    );
  }

  /**
   * Fetches a player's profile flags for given ID.
   * This includes Platform Bans, and Opt-out statuses.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiProfileIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiProfileIdGet$Json(params: {

    /**
     * Player ID to fetch profile flags from.
     */
    id: number;
    context?: HttpContext
  }
): Observable<UserProfileFlagsDto> {

    return this.apiProfileIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<UserProfileFlagsDto>) => r.body as UserProfileFlagsDto)
    );
  }

  /**
   * Path part for operation apiProfilePut
   */
  static readonly ApiProfilePutPath = '/api/Profile';

  /**
   * Updates user-settable profile values.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiProfilePut$Plain()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiProfilePut$Plain$Response(params?: {
    context?: HttpContext

    /**
     * Updated profile values to set on profile.
     * Note: Platform Ban state cannot be edited through this endpoint.
     */
    body?: UserProfileFlagsDto
  }
): Observable<StrictHttpResponse<UserProfileFlagsDto>> {

    const rb = new RequestBuilder(this.rootUrl, ProfileService.ApiProfilePutPath, 'put');
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
        return r as StrictHttpResponse<UserProfileFlagsDto>;
      })
    );
  }

  /**
   * Updates user-settable profile values.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiProfilePut$Plain$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiProfilePut$Plain(params?: {
    context?: HttpContext

    /**
     * Updated profile values to set on profile.
     * Note: Platform Ban state cannot be edited through this endpoint.
     */
    body?: UserProfileFlagsDto
  }
): Observable<UserProfileFlagsDto> {

    return this.apiProfilePut$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<UserProfileFlagsDto>) => r.body as UserProfileFlagsDto)
    );
  }

  /**
   * Updates user-settable profile values.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiProfilePut$Json()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiProfilePut$Json$Response(params?: {
    context?: HttpContext

    /**
     * Updated profile values to set on profile.
     * Note: Platform Ban state cannot be edited through this endpoint.
     */
    body?: UserProfileFlagsDto
  }
): Observable<StrictHttpResponse<UserProfileFlagsDto>> {

    const rb = new RequestBuilder(this.rootUrl, ProfileService.ApiProfilePutPath, 'put');
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
        return r as StrictHttpResponse<UserProfileFlagsDto>;
      })
    );
  }

  /**
   * Updates user-settable profile values.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiProfilePut$Json$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiProfilePut$Json(params?: {
    context?: HttpContext

    /**
     * Updated profile values to set on profile.
     * Note: Platform Ban state cannot be edited through this endpoint.
     */
    body?: UserProfileFlagsDto
  }
): Observable<UserProfileFlagsDto> {

    return this.apiProfilePut$Json$Response(params).pipe(
      map((r: StrictHttpResponse<UserProfileFlagsDto>) => r.body as UserProfileFlagsDto)
    );
  }

}
