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

import { ReplayDto } from '../models/replay-dto';

@Injectable({
  providedIn: 'root',
})
export class ReplayService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiReplayGet
   */
  static readonly ApiReplayGetPath = '/api/Replay';

  /**
   * Lists all replays by ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayGet$Plain$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<string>>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<string>>;
      })
    );
  }

  /**
   * Lists all replays by ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayGet$Plain(params?: {
    context?: HttpContext
  }
): Observable<Array<string>> {

    return this.apiReplayGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<string>>) => r.body as Array<string>)
    );
  }

  /**
   * Lists all replays by ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayGet$Json$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<string>>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayGetPath, 'get');
    if (params) {
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<string>>;
      })
    );
  }

  /**
   * Lists all replays by ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayGet$Json(params?: {
    context?: HttpContext
  }
): Observable<Array<string>> {

    return this.apiReplayGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<string>>) => r.body as Array<string>)
    );
  }

  /**
   * Path part for operation apiReplayReplayIdGet
   */
  static readonly ApiReplayReplayIdGetPath = '/api/Replay/{replayId}';

  /**
   * Gets Replay data for given Replay ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayReplayIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReplayIdGet$Plain$Response(params: {

    /**
     * ID of Replay to fetch
     */
    replayId: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<ReplayDto>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayReplayIdGetPath, 'get');
    if (params) {
      rb.path('replayId', params.replayId, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<ReplayDto>;
      })
    );
  }

  /**
   * Gets Replay data for given Replay ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayReplayIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReplayIdGet$Plain(params: {

    /**
     * ID of Replay to fetch
     */
    replayId: string;
    context?: HttpContext
  }
): Observable<ReplayDto> {

    return this.apiReplayReplayIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<ReplayDto>) => r.body as ReplayDto)
    );
  }

  /**
   * Gets Replay data for given Replay ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayReplayIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReplayIdGet$Json$Response(params: {

    /**
     * ID of Replay to fetch
     */
    replayId: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<ReplayDto>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayReplayIdGetPath, 'get');
    if (params) {
      rb.path('replayId', params.replayId, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<ReplayDto>;
      })
    );
  }

  /**
   * Gets Replay data for given Replay ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayReplayIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReplayIdGet$Json(params: {

    /**
     * ID of Replay to fetch
     */
    replayId: string;
    context?: HttpContext
  }
): Observable<ReplayDto> {

    return this.apiReplayReplayIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<ReplayDto>) => r.body as ReplayDto)
    );
  }

  /**
   * Path part for operation apiReplayPostIdPost
   */
  static readonly ApiReplayPostIdPostPath = '/api/Replay/{postId}';

  /**
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayPostIdPost()` instead.
   *
   * This method sends `multipart/form-data` and handles request body of type `multipart/form-data`.
   */
  apiReplayPostIdPost$Response(params: {
    postId: string;
    ignoreChecks?: boolean;
    context?: HttpContext
    body?: {
'replay'?: Blob;
}
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayPostIdPostPath, 'post');
    if (params) {
      rb.path('postId', params.postId, {});
      rb.query('ignoreChecks', params.ignoreChecks, {});
      rb.body(params.body, 'multipart/form-data');
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
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayPostIdPost$Response()` instead.
   *
   * This method sends `multipart/form-data` and handles request body of type `multipart/form-data`.
   */
  apiReplayPostIdPost(params: {
    postId: string;
    ignoreChecks?: boolean;
    context?: HttpContext
    body?: {
'replay'?: Blob;
}
  }
): Observable<void> {

    return this.apiReplayPostIdPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiReplayReprocessAllPatch
   */
  static readonly ApiReplayReprocessAllPatchPath = '/api/Replay/reprocess/all';

  /**
   * Triggers reprocessing on all replays within the specified date/time range (Usable only by Administrators).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayReprocessAllPatch()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReprocessAllPatch$Response(params?: {

    /**
     * Start of date/time range
     */
    start?: string;

    /**
     * End of date/time range
     */
    end?: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayReprocessAllPatchPath, 'patch');
    if (params) {
      rb.query('start', params.start, {});
      rb.query('end', params.end, {});
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
   * Triggers reprocessing on all replays within the specified date/time range (Usable only by Administrators).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayReprocessAllPatch$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReprocessAllPatch(params?: {

    /**
     * Start of date/time range
     */
    start?: string;

    /**
     * End of date/time range
     */
    end?: string;
    context?: HttpContext
  }
): Observable<void> {

    return this.apiReplayReprocessAllPatch$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiReplayReprocessReplayIdPatch
   */
  static readonly ApiReplayReprocessReplayIdPatchPath = '/api/Replay/reprocess/{replayId}';

  /**
   * Triggers reporessing on a replay (Usable only by Administrators).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiReplayReprocessReplayIdPatch()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReprocessReplayIdPatch$Response(params: {
    replayId: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, ReplayService.ApiReplayReprocessReplayIdPatchPath, 'patch');
    if (params) {
      rb.path('replayId', params.replayId, {});
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
   * Triggers reporessing on a replay (Usable only by Administrators).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiReplayReprocessReplayIdPatch$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiReplayReprocessReplayIdPatch(params: {
    replayId: string;
    context?: HttpContext
  }
): Observable<void> {

    return this.apiReplayReprocessReplayIdPatch$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
