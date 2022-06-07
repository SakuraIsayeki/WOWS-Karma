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

import { PlatformBanDto } from '../models/platform-ban-dto';

@Injectable({
  providedIn: 'root',
})
export class PlatformBansService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiModBansUserIdGet
   */
  static readonly ApiModBansUserIdGetPath = '/api/mod/bans/{userId}';

  /**
   * Fetches all bans emitted for a specific user.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModBansUserIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansUserIdGet$Plain$Response(params: {

    /**
     * ID of user account
     */
    userId: number;

    /**
     * Return only currently active platform bans.
     */
    currentOnly?: boolean;
  }): Observable<StrictHttpResponse<Array<PlatformBanDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlatformBansService.ApiModBansUserIdGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('currentOnly', params.currentOnly, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlatformBanDto>>;
      })
    );
  }

  /**
   * Fetches all bans emitted for a specific user.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModBansUserIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansUserIdGet$Plain(params: {

    /**
     * ID of user account
     */
    userId: number;

    /**
     * Return only currently active platform bans.
     */
    currentOnly?: boolean;
  }): Observable<Array<PlatformBanDto>> {

    return this.apiModBansUserIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlatformBanDto>>) => r.body as Array<PlatformBanDto>)
    );
  }

  /**
   * Fetches all bans emitted for a specific user.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModBansUserIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansUserIdGet$Json$Response(params: {

    /**
     * ID of user account
     */
    userId: number;

    /**
     * Return only currently active platform bans.
     */
    currentOnly?: boolean;
  }): Observable<StrictHttpResponse<Array<PlatformBanDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PlatformBansService.ApiModBansUserIdGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('currentOnly', params.currentOnly, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlatformBanDto>>;
      })
    );
  }

  /**
   * Fetches all bans emitted for a specific user.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModBansUserIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansUserIdGet$Json(params: {

    /**
     * ID of user account
     */
    userId: number;

    /**
     * Return only currently active platform bans.
     */
    currentOnly?: boolean;
  }): Observable<Array<PlatformBanDto>> {

    return this.apiModBansUserIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlatformBanDto>>) => r.body as Array<PlatformBanDto>)
    );
  }

  /**
   * Path part for operation apiModBansPost
   */
  static readonly ApiModBansPostPath = '/api/mod/bans';

  /**
   * Emits a new Platform Ban.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModBansPost()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiModBansPost$Response(params?: {

    /**
     * (Helper) Sets a temporary ban, to the number of specified days starting from UTC now.
     */
    days?: number;

    /**
     * Platform Ban to emit
     */
    body?: PlatformBanDto
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PlatformBansService.ApiModBansPostPath, 'post');
    if (params) {
      rb.query('days', params.days, {});
      rb.body(params.body, 'application/*+json');
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
   * Emits a new Platform Ban.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModBansPost$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiModBansPost(params?: {

    /**
     * (Helper) Sets a temporary ban, to the number of specified days starting from UTC now.
     */
    days?: number;

    /**
     * Platform Ban to emit
     */
    body?: PlatformBanDto
  }): Observable<void> {

    return this.apiModBansPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiModBansIdDelete
   */
  static readonly ApiModBansIdDeletePath = '/api/mod/bans/{id}';

  /**
   * Reverts Platform Ban with specified ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModBansIdDelete()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansIdDelete$Response(params: {

    /**
     * ID of Platform Ban to revert.
     */
    id?: string;
    id: string;
  }): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PlatformBansService.ApiModBansIdDeletePath, 'delete');
    if (params) {
      rb.query('id', params.id, {});
      rb.path('id', params.id, {});
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
   * Reverts Platform Ban with specified ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModBansIdDelete$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModBansIdDelete(params: {

    /**
     * ID of Platform Ban to revert.
     */
    id?: string;
    id: string;
  }): Observable<void> {

    return this.apiModBansIdDelete$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
