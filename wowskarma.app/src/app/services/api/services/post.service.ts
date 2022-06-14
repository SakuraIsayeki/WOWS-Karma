/* tslint:disable */
import { HttpClient, HttpResponse } from "@angular/common/http";
/* eslint-disable */
import { Injectable } from "@angular/core";
import { Observable } from "rxjs";
import { filter, map } from "rxjs/operators";
import { ApiConfiguration } from "../api-configuration";
import { BaseService } from "../base-service";

import { PlayerPostDto } from "../models/player-post-dto";
import { RequestBuilder } from "../request-builder";
import { StrictHttpResponse } from "../strict-http-response";

@Injectable({
    providedIn: "root",
})
export class PostService extends BaseService {
    /**
     * Path part for operation apiPostPostIdGet
     */
    static readonly ApiPostPostIdGetPath = "/api/Post/{postId}";
    /**
     * Path part for operation apiPostPostIdDelete
     */
    static readonly ApiPostPostIdDeletePath = "/api/Post/{postId}";
    /**
     * Path part for operation apiPostUserIdReceivedGet
     */
    static readonly ApiPostUserIdReceivedGetPath = "/api/Post/{userId}/received";
    /**
     * Path part for operation apiPostUserIdSentGet
     */
    static readonly ApiPostUserIdSentGetPath = "/api/Post/{userId}/sent";
    /**
     * Path part for operation apiPostLatestGet
     */
    static readonly ApiPostLatestGetPath = "/api/Post/latest";
    /**
     * Path part for operation apiPostPut
     */
    static readonly ApiPostPutPath = "/api/Post";
    /**
     * Path part for operation apiPostPost
     */
    static readonly ApiPostPostPath = "/api/Post";

    constructor(
        config: ApiConfiguration,
        http: HttpClient,
    ) {
        super(config, http);
    }

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
    }): Observable<StrictHttpResponse<PlayerPostDto>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdGetPath, "get");
        if (params) {
            rb.path("postId", params.postId, {});
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "text/plain",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<PlayerPostDto>;
            }),
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
    }): Observable<PlayerPostDto> {

        return this.apiPostPostIdGet$Plain$Response(params).pipe(
            map((r: StrictHttpResponse<PlayerPostDto>) => r.body as PlayerPostDto),
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
    }): Observable<StrictHttpResponse<PlayerPostDto>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdGetPath, "get");
        if (params) {
            rb.path("postId", params.postId, {});
        }

        return this.http.request(rb.build({
            responseType: "json",
            accept: "text/json",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<PlayerPostDto>;
            }),
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
    }): Observable<PlayerPostDto> {

        return this.apiPostPostIdGet$Json$Response(params).pipe(
            map((r: StrictHttpResponse<PlayerPostDto>) => r.body as PlayerPostDto),
        );
    }

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
    }): Observable<StrictHttpResponse<void>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostIdDeletePath, "delete");
        if (params) {
            rb.path("postId", params.postId, {});
            rb.query("ignoreChecks", params.ignoreChecks, {});
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "*/*",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
            }),
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
    }): Observable<void> {

        return this.apiPostPostIdDelete$Response(params).pipe(
            map((r: StrictHttpResponse<void>) => r.body as void),
        );
    }

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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdReceivedGetPath, "get");
        if (params) {
            rb.path("userId", params.userId, {});
            rb.query("lastResults", params.lastResults, {});
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "text/plain",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostUserIdReceivedGet$Plain$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdReceivedGetPath, "get");
        if (params) {
            rb.path("userId", params.userId, {});
            rb.query("lastResults", params.lastResults, {});
        }

        return this.http.request(rb.build({
            responseType: "json",
            accept: "text/json",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostUserIdReceivedGet$Json$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
        );
    }

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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdSentGetPath, "get");
        if (params) {
            rb.path("userId", params.userId, {});
            rb.query("lastResults", params.lastResults, {});
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "text/plain",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostUserIdSentGet$Plain$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostUserIdSentGetPath, "get");
        if (params) {
            rb.path("userId", params.userId, {});
            rb.query("lastResults", params.lastResults, {});
        }

        return this.http.request(rb.build({
            responseType: "json",
            accept: "text/json",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results (where available)
         */
        lastResults?: number;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostUserIdSentGet$Json$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
        );
    }

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
         * Return maximum of results.
         */
        count?: number;

        /**
         * Filters returned posts by Replay attachment.
         */
        hasReplay?: boolean;

        /**
         * Hides posts containing Mod Actions (visible only to CMs).
         */
        hideModActions?: boolean;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostLatestGetPath, "get");
        if (params) {
            rb.query("count", params.count, {});
            rb.query("hasReplay", params.hasReplay, {});
            rb.query("hideModActions", params.hideModActions, {});
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "text/plain",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results.
         */
        count?: number;

        /**
         * Filters returned posts by Replay attachment.
         */
        hasReplay?: boolean;

        /**
         * Hides posts containing Mod Actions (visible only to CMs).
         */
        hideModActions?: boolean;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostLatestGet$Plain$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
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
         * Return maximum of results.
         */
        count?: number;

        /**
         * Filters returned posts by Replay attachment.
         */
        hasReplay?: boolean;

        /**
         * Hides posts containing Mod Actions (visible only to CMs).
         */
        hideModActions?: boolean;
    }): Observable<StrictHttpResponse<Array<PlayerPostDto>>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostLatestGetPath, "get");
        if (params) {
            rb.query("count", params.count, {});
            rb.query("hasReplay", params.hasReplay, {});
            rb.query("hideModActions", params.hideModActions, {});
        }

        return this.http.request(rb.build({
            responseType: "json",
            accept: "text/json",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return r as StrictHttpResponse<Array<PlayerPostDto>>;
            }),
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
         * Return maximum of results.
         */
        count?: number;

        /**
         * Filters returned posts by Replay attachment.
         */
        hasReplay?: boolean;

        /**
         * Hides posts containing Mod Actions (visible only to CMs).
         */
        hideModActions?: boolean;
    }): Observable<Array<PlayerPostDto>> {

        return this.apiPostLatestGet$Json$Response(params).pipe(
            map((r: StrictHttpResponse<Array<PlayerPostDto>>) => r.body as Array<PlayerPostDto>),
        );
    }

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

        /**
         * Post object to submit
         */
        body?: PlayerPostDto
    }): Observable<StrictHttpResponse<void>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPutPath, "put");
        if (params) {
            rb.query("ignoreChecks", params.ignoreChecks, {});
            rb.body(params.body, "application/*+json");
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "*/*",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
            }),
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

        /**
         * Post object to submit
         */
        body?: PlayerPostDto
    }): Observable<void> {

        return this.apiPostPut$Response(params).pipe(
            map((r: StrictHttpResponse<void>) => r.body as void),
        );
    }

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
        body?: {
            "postDto"?: string;
            "replay"?: File | null;
        }
    }): Observable<StrictHttpResponse<void>> {

        const rb = new RequestBuilder(this.rootUrl, PostService.ApiPostPostPath, "post");
        if (params) {
            rb.query("ignoreChecks", params.ignoreChecks, {});
            rb.body(params.body, "multipart/form-data");
        }

        return this.http.request(rb.build({
            responseType: "text",
            accept: "*/*",
        })).pipe(
            filter((r: any) => r instanceof HttpResponse),
            map((r: HttpResponse<any>) => {
                return (r as HttpResponse<any>).clone({ body: undefined }) as StrictHttpResponse<void>;
            }),
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
        body?: {
            "postDto"?: string;
            "replay"?: File | null;
        }
    }): Observable<void> {

        return this.apiPostPost$Response(params).pipe(
            map((r: StrictHttpResponse<void>) => r.body as void),
        );
    }

}
