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

import { PlayerPostDto } from '../models/player-post-dto';

@Injectable({
  providedIn: 'root',
})
export class PostService extends BaseService {

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {
    const config = inject(ApiConfiguration);
    const http = inject(HttpClient);

    super(config, http);
  }

  /**
   * Path part for operation apiPostGet
   */
  static readonly ApiPostGetPath = '/api/Post';

  /**
   * Lists all post IDs.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostGet$Plain$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<string>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostGetPath, 'get');
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
   * Lists all post IDs.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostGet$Plain(params?: {
    context?: HttpContext
  }
): Observable<Array<string>> {

    return this.apiPostGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<string>>) => r.body as Array<string>)
    );
  }

  /**
   * Lists all post IDs.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostGet$Json$Response(params?: {
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<string>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostGetPath, 'get');
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
   * Lists all post IDs.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostGet$Json(params?: {
    context?: HttpContext
  }
): Observable<Array<string>> {

    return this.apiPostGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<string>>) => r.body as Array<string>)
    );
  }

  /**
   * Path part for operation apiPostPut
   */
  static readonly ApiPostPutPath = '/api/Post';

  /**
   * Submits an updated post for editing.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostPut()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPostPut$Response(params?: {

    /**
     * Bypass API Validation for post editing (Admin only).
     */
    ignoreChecks?: boolean;
    context?: HttpContext

    /**
     * Post object to submit
     */
    body?: PlayerPostDto
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPutPath, 'put');
    if (params) {
      rb.query('ignoreChecks', params.ignoreChecks, {});
      rb.body(params.body, 'application/*+json');
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
   * Submits an updated post for editing.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostPut$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiPostPut(params?: {

    /**
     * Bypass API Validation for post editing (Admin only).
     */
    ignoreChecks?: boolean;
    context?: HttpContext

    /**
     * Post object to submit
     */
    body?: PlayerPostDto
  }
): Observable<void> {

    return this.apiPostPut$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiPostPost
   */
  static readonly ApiPostPostPath = '/api/Post';

  /**
   * Submits a new post for creation.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostPost()` instead.
   *
   * This method sends `multipart/form-data` and handles request body of type `multipart/form-data`.
   */
  apiPostPost$Response(params?: {

    /**
     * Bypass API Validation for post creation (Admin only)
     */
    ignoreChecks?: boolean;
    context?: HttpContext
    body?: {
'postDto'?: string;
'replay'?: Blob;
}
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostPath, 'post');
    if (params) {
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
   * Submits a new post for creation.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostPost$Response()` instead.
   *
   * This method sends `multipart/form-data` and handles request body of type `multipart/form-data`.
   */
  apiPostPost(params?: {

    /**
     * Bypass API Validation for post creation (Admin only)
     */
    ignoreChecks?: boolean;
    context?: HttpContext
    body?: {
'postDto'?: string;
'replay'?: Blob;
}
  }
): Observable<void> {

    return this.apiPostPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiPostPostIdGet
   */
  static readonly ApiPostPostIdGetPath = '/api/Post/{postId}';

  /**
   * Fetches player post with given ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostPostIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdGet$Plain$Response(params: {

    /**
     * Post&#x27;s GUID
     */
    postId: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PlayerPostDto>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdGetPath, 'get');
    if (params) {
      rb.path('postId', params.postId, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerPostDto>;
      })
    );
  }

  /**
   * Fetches player post with given ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostPostIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdGet$Plain(params: {

    /**
     * Post&#x27;s GUID
     */
    postId: string;
    context?: HttpContext
  }
): Observable<PlayerPostDto> {

    return this.apiPostPostIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerPostDto>) => r.body as PlayerPostDto)
    );
  }

  /**
   * Fetches player post with given ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostPostIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdGet$Json$Response(params: {

    /**
     * Post&#x27;s GUID
     */
    postId: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PlayerPostDto>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdGetPath, 'get');
    if (params) {
      rb.path('postId', params.postId, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<PlayerPostDto>;
      })
    );
  }

  /**
   * Fetches player post with given ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostPostIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdGet$Json(params: {

    /**
     * Post&#x27;s GUID
     */
    postId: string;
    context?: HttpContext
  }
): Observable<PlayerPostDto> {

    return this.apiPostPostIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<PlayerPostDto>) => r.body as PlayerPostDto)
    );
  }

  /**
   * Path part for operation apiPostPostIdDelete
   */
  static readonly ApiPostPostIdDeletePath = '/api/Post/{postId}';

  /**
   * Requests a post deletion.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostPostIdDelete()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdDelete$Response(params: {

    /**
     * ID of Post to delete
     */
    postId: string;

    /**
     * Bypass API Validation for post deletion (Admin only).
     */
    ignoreChecks?: boolean;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdDeletePath, 'delete');
    if (params) {
      rb.path('postId', params.postId, {});
      rb.query('ignoreChecks', params.ignoreChecks, {});
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
   * Requests a post deletion.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostPostIdDelete$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostPostIdDelete(params: {

    /**
     * ID of Post to delete
     */
    postId: string;

    /**
     * Bypass API Validation for post deletion (Admin only).
     */
    ignoreChecks?: boolean;
    context?: HttpContext
  }
): Observable<void> {

    return this.apiPostPostIdDelete$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiPostUserIdReceivedGet
   */
  static readonly ApiPostUserIdReceivedGetPath = '/api/Post/{userId}/received';

  /**
   * Fetches all posts that a given player has received.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostUserIdReceivedGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdReceivedGet$Plain$Response(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdReceivedGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches all posts that a given player has received.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostUserIdReceivedGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdReceivedGet$Plain(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostUserIdReceivedGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

  /**
   * Fetches all posts that a given player has received.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostUserIdReceivedGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdReceivedGet$Json$Response(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdReceivedGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches all posts that a given player has received.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostUserIdReceivedGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdReceivedGet$Json(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostUserIdReceivedGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

  /**
   * Path part for operation apiPostUserIdSentGet
   */
  static readonly ApiPostUserIdSentGetPath = '/api/Post/{userId}/sent';

  /**
   * Fetches all posts that a given player has sent.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostUserIdSentGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdSentGet$Plain$Response(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdSentGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches all posts that a given player has sent.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostUserIdSentGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdSentGet$Plain(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostUserIdSentGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

  /**
   * Fetches all posts that a given player has sent.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostUserIdSentGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdSentGet$Json$Response(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdSentGetPath, 'get');
    if (params) {
      rb.path('userId', params.userId, {});
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches all posts that a given player has sent.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostUserIdSentGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostUserIdSentGet$Json(params: {

    /**
     * Account ID of player to query
     */
    userId: number;

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostUserIdSentGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

  /**
   * Path part for operation apiPostLatestGet
   */
  static readonly ApiPostLatestGetPath = '/api/Post/latest';

  /**
   * Fetches latest posts.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostLatestGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostLatestGet$Plain$Response(params?: {

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;

    /**
     * Filters returned posts by Replay attachment.
     */
    hasReplay?: boolean;

    /**
     * Hides posts containing Mod Actions (visible only to CMs).
     */
    hideModActions?: boolean;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostLatestGetPath, 'get');
    if (params) {
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
      rb.query('hasReplay', params.hasReplay, {});
      rb.query('hideModActions', params.hideModActions, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches latest posts.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostLatestGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostLatestGet$Plain(params?: {

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;

    /**
     * Filters returned posts by Replay attachment.
     */
    hasReplay?: boolean;

    /**
     * Hides posts containing Mod Actions (visible only to CMs).
     */
    hideModActions?: boolean;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostLatestGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

  /**
   * Fetches latest posts.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiPostLatestGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostLatestGet$Json$Response(params?: {

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;

    /**
     * Filters returned posts by Replay attachment.
     */
    hasReplay?: boolean;

    /**
     * Hides posts containing Mod Actions (visible only to CMs).
     */
    hideModActions?: boolean;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

    const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostLatestGetPath, 'get');
    if (params) {
      rb.query('page', params.page, {});
      rb.query('pageSize', params.pageSize, {});
      rb.query('hasReplay', params.hasReplay, {});
      rb.query('hideModActions', params.hideModActions, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PlayerPostDto>>;
      })
    );
  }

  /**
   * Fetches latest posts.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiPostLatestGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiPostLatestGet$Json(params?: {

    /**
     * Page number to fetch
     */
    page?: number;

    /**
     * Number of posts per page
     */
    pageSize?: number;

    /**
     * Filters returned posts by Replay attachment.
     */
    hasReplay?: boolean;

    /**
     * Hides posts containing Mod Actions (visible only to CMs).
     */
    hideModActions?: boolean;
    context?: HttpContext
  }
): Observable<Array<PlayerPostDto>> {

    return this.apiPostLatestGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>)
    );
  }

}
