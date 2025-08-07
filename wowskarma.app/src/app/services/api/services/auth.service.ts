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


@Injectable({
  providedIn: 'root',
})
export class AuthService extends BaseService {

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {
    const config = inject(ApiConfiguration);
    const http = inject(HttpClient);

    super(config, http);
  }

  /**
   * Path part for operation apiAuthHead
   */
  static readonly ApiAuthHeadPath = '/api/Auth';

  /**
   * Verifies current request's Authentication to API.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthHead()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthHead$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthHeadPath, 'head');
    if (params) {
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
   * Verifies current request's Authentication to API.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthHead$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthHead(params?: {
    context?: HttpContext
  }
): Observable<void> {

    return this.apiAuthHead$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiAuthLoginGet
   */
  static readonly ApiAuthLoginGetPath = '/api/Auth/login';

  /**
   * Provides redirection to Wargaming OpenID Authentication.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthLoginGet()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthLoginGet$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthLoginGetPath, 'get');
    if (params) {
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
   * Provides redirection to Wargaming OpenID Authentication.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthLoginGet$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthLoginGet(params?: {
    context?: HttpContext
  }
): Observable<void> {

    return this.apiAuthLoginGet$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiAuthWgCallbackGet
   */
  static readonly ApiAuthWgCallbackGetPath = '/api/Auth/wg-callback';

  /**
   * Provides a callback endpoint for Wargaming OpenID results, and stores all authentication information to relevant cookies.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthWgCallbackGet()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthWgCallbackGet$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthWgCallbackGetPath, 'get');
    if (params) {
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
   * Provides a callback endpoint for Wargaming OpenID results, and stores all authentication information to relevant cookies.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthWgCallbackGet$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthWgCallbackGet(params?: {
    context?: HttpContext
  }
): Observable<void> {

    return this.apiAuthWgCallbackGet$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiAuthRenewSeedPost
   */
  static readonly ApiAuthRenewSeedPostPath = '/api/Auth/renew-seed';

  /**
   * Renews Seed-Token, invalidating all previously issued JWTs.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthRenewSeedPost()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRenewSeedPost$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthRenewSeedPostPath, 'post');
    if (params) {
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
   * Renews Seed-Token, invalidating all previously issued JWTs.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthRenewSeedPost$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRenewSeedPost(params?: {
    context?: HttpContext
  }
): Observable<void> {

    return this.apiAuthRenewSeedPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiAuthRefreshTokenGet
   */
  static readonly ApiAuthRefreshTokenGetPath = '/api/Auth/refresh-token';

  /**
   * Issues a new JWT with information mirrored to current token.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthRefreshTokenGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRefreshTokenGet$Plain$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<string>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthRefreshTokenGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<string>;
      })
    );
  }

  /**
   * Issues a new JWT with information mirrored to current token.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthRefreshTokenGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRefreshTokenGet$Plain(params?: {
    context?: HttpContext
  }
): Observable<string> {

    return this.apiAuthRefreshTokenGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<string>) => r.body as string)
    );
  }

  /**
   * Issues a new JWT with information mirrored to current token.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiAuthRefreshTokenGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRefreshTokenGet$Json$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<string>> {

    const rb = new RequestBuilder(this.rootUrl, AuthService.ApiAuthRefreshTokenGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<string>;
      })
    );
  }

  /**
   * Issues a new JWT with information mirrored to current token.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiAuthRefreshTokenGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiAuthRefreshTokenGet$Json(params?: {
    context?: HttpContext
  }
): Observable<string> {

    return this.apiAuthRefreshTokenGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<string>) => r.body as string)
    );
  }

}
