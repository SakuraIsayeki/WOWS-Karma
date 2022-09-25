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

import { PostModActionDto } from '../models/post-mod-action-dto';

@Injectable({
  providedIn: 'root',
})
export class ModActionService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiModActionIdGet
   */
  static readonly ApiModActionIdGetPath = '/api/mod/action/{id}';

  /**
   * Fetches ModAction by ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdGet$Plain$Response(params: {

    /**
     * ID of ModAction to fetch.
     */
    id: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PostModActionDto>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionIdGetPath, 'get');
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
        return r as StrictHttpResponse<PostModActionDto>;
      })
    );
  }

  /**
   * Fetches ModAction by ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdGet$Plain(params: {

    /**
     * ID of ModAction to fetch.
     */
    id: string;
    context?: HttpContext
  }
): Observable<PostModActionDto> {

    return this.apiModActionIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<PostModActionDto>) => r.body as PostModActionDto)
    );
  }

  /**
   * Fetches ModAction by ID.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdGet$Json$Response(params: {

    /**
     * ID of ModAction to fetch.
     */
    id: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<PostModActionDto>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionIdGetPath, 'get');
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
        return r as StrictHttpResponse<PostModActionDto>;
      })
    );
  }

  /**
   * Fetches ModAction by ID.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdGet$Json(params: {

    /**
     * ID of ModAction to fetch.
     */
    id: string;
    context?: HttpContext
  }
): Observable<PostModActionDto> {

    return this.apiModActionIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<PostModActionDto>) => r.body as PostModActionDto)
    );
  }

  /**
   * Path part for operation apiModActionIdDelete
   */
  static readonly ApiModActionIdDeletePath = '/api/mod/action/{id}';

  /**
   * Reverts an existing ModAction.
   *
   * Usable only by Community Managers and Administrators.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionIdDelete()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdDelete$Response(params: {

    /**
     * ID of ModAction to delete.
     */
    id: string;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionIdDeletePath, 'delete');
    if (params) {
      rb.path('id', params.id, {});
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
   * Reverts an existing ModAction.
   *
   * Usable only by Community Managers and Administrators.
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionIdDelete$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionIdDelete(params: {

    /**
     * ID of ModAction to delete.
     */
    id: string;
    context?: HttpContext
  }
): Observable<void> {

    return this.apiModActionIdDelete$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

  /**
   * Path part for operation apiModActionListGet
   */
  static readonly ApiModActionListGetPath = '/api/mod/action/list';

  /**
   * Lists ModActions by Post or User IDs.
   *
   * User ID gets ignored if a Post ID is provided.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionListGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionListGet$Plain$Response(params?: {

    /**
     * Get ModActions for specific Post.
     */
    postId?: string;

    /**
     * Get ModActions for specific User.
     */
    userId?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PostModActionDto>>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionListGetPath, 'get');
    if (params) {
      rb.query('postId', params.postId, {});
      rb.query('userId', params.userId, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PostModActionDto>>;
      })
    );
  }

  /**
   * Lists ModActions by Post or User IDs.
   *
   * User ID gets ignored if a Post ID is provided.
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionListGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionListGet$Plain(params?: {

    /**
     * Get ModActions for specific Post.
     */
    postId?: string;

    /**
     * Get ModActions for specific User.
     */
    userId?: number;
    context?: HttpContext
  }
): Observable<Array<PostModActionDto>> {

    return this.apiModActionListGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PostModActionDto>>) => r.body as Array<PostModActionDto>)
    );
  }

  /**
   * Lists ModActions by Post or User IDs.
   *
   * User ID gets ignored if a Post ID is provided.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionListGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionListGet$Json$Response(params?: {

    /**
     * Get ModActions for specific Post.
     */
    postId?: string;

    /**
     * Get ModActions for specific User.
     */
    userId?: number;
    context?: HttpContext
  }
): Observable<StrictHttpResponse<Array<PostModActionDto>>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionListGetPath, 'get');
    if (params) {
      rb.query('postId', params.postId, {});
      rb.query('userId', params.userId, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json',
      context: params?.context
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<PostModActionDto>>;
      })
    );
  }

  /**
   * Lists ModActions by Post or User IDs.
   *
   * User ID gets ignored if a Post ID is provided.
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionListGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiModActionListGet$Json(params?: {

    /**
     * Get ModActions for specific Post.
     */
    postId?: string;

    /**
     * Get ModActions for specific User.
     */
    userId?: number;
    context?: HttpContext
  }
): Observable<Array<PostModActionDto>> {

    return this.apiModActionListGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<PostModActionDto>>) => r.body as Array<PostModActionDto>)
    );
  }

  /**
   * Path part for operation apiModActionPost
   */
  static readonly ApiModActionPostPath = '/api/mod/action';

  /**
   * Submits a new ModAction.
   *
   * Usable only by Community Managers.
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiModActionPost()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiModActionPost$Response(params?: {
    context?: HttpContext

    /**
     * ModAction to submit
     */
    body?: PostModActionDto
  }
): Observable<StrictHttpResponse<void>> {

    const rb = new RequestBuilder(this.rootUrl, ModActionService.ApiModActionPostPath, 'post');
    if (params) {
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
   * Submits a new ModAction.
   *
   * Usable only by Community Managers.
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiModActionPost$Response()` instead.
   *
   * This method sends `application/*+json` and handles request body of type `application/*+json`.
   */
  apiModActionPost(params?: {
    context?: HttpContext

    /**
     * ModAction to submit
     */
    body?: PostModActionDto
  }
): Observable<void> {

    return this.apiModActionPost$Response(params).pipe(
      map((r: StrictHttpResponse<void>) => r.body as void)
    );
  }

}
