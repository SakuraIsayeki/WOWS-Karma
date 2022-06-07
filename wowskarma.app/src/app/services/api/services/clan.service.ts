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

import { ClanListingDto } from '../models/clan-listing-dto';
import { ClanProfileFullDto } from '../models/clan-profile-full-dto';

@Injectable({
  providedIn: 'root',
})
export class ClanService extends BaseService {
  constructor(
    config: ApiConfiguration,
    http: HttpClient
  ) {
    super(config, http);
  }

  /**
   * Path part for operation apiClanClanIdGet
   */
  static readonly ApiClanClanIdGetPath = '/api/Clan/{clanId}';

  /**
   * Fetches Clan info for a given Clan ID, along with clan members (unless excluded).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiClanClanIdGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanClanIdGet$Plain$Response(params: {

    /**
     * ID of Clan to fetch info/members for
     */
    clanId: number;

    /**
     * Select whether response must include members
     */
    includeMembers?: boolean;
  }): Observable<StrictHttpResponse<ClanProfileFullDto>> {

    const rb = new RequestBuilder(this.rootUrl, ClanService.ApiClanClanIdGetPath, 'get');
    if (params) {
      rb.path('clanId', params.clanId, {});
      rb.query('includeMembers', params.includeMembers, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<ClanProfileFullDto>;
      })
    );
  }

  /**
   * Fetches Clan info for a given Clan ID, along with clan members (unless excluded).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiClanClanIdGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanClanIdGet$Plain(params: {

    /**
     * ID of Clan to fetch info/members for
     */
    clanId: number;

    /**
     * Select whether response must include members
     */
    includeMembers?: boolean;
  }): Observable<ClanProfileFullDto> {

    return this.apiClanClanIdGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<ClanProfileFullDto>) => r.body as ClanProfileFullDto)
    );
  }

  /**
   * Fetches Clan info for a given Clan ID, along with clan members (unless excluded).
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiClanClanIdGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanClanIdGet$Json$Response(params: {

    /**
     * ID of Clan to fetch info/members for
     */
    clanId: number;

    /**
     * Select whether response must include members
     */
    includeMembers?: boolean;
  }): Observable<StrictHttpResponse<ClanProfileFullDto>> {

    const rb = new RequestBuilder(this.rootUrl, ClanService.ApiClanClanIdGetPath, 'get');
    if (params) {
      rb.path('clanId', params.clanId, {});
      rb.query('includeMembers', params.includeMembers, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<ClanProfileFullDto>;
      })
    );
  }

  /**
   * Fetches Clan info for a given Clan ID, along with clan members (unless excluded).
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiClanClanIdGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanClanIdGet$Json(params: {

    /**
     * ID of Clan to fetch info/members for
     */
    clanId: number;

    /**
     * Select whether response must include members
     */
    includeMembers?: boolean;
  }): Observable<ClanProfileFullDto> {

    return this.apiClanClanIdGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<ClanProfileFullDto>) => r.body as ClanProfileFullDto)
    );
  }

  /**
   * Path part for operation apiClanSearchSearchGet
   */
  static readonly ApiClanSearchSearchGetPath = '/api/Clan/search/{search}';

  /**
   * Searches all clans relevant to a given search string.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiClanSearchSearchGet$Plain()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanSearchSearchGet$Plain$Response(params: {

    /**
     * Search query (Clan Tag and/or Name)
     */
    search: string;

    /**
     * Amount of maximum results to return
     */
    results?: number;
  }): Observable<StrictHttpResponse<Array<ClanListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, ClanService.ApiClanSearchSearchGetPath, 'get');
    if (params) {
      rb.path('search', params.search, {});
      rb.query('results', params.results, {});
    }

    return this.http.request(rb.build({
      responseType: 'text',
      accept: 'text/plain'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<ClanListingDto>>;
      })
    );
  }

  /**
   * Searches all clans relevant to a given search string.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiClanSearchSearchGet$Plain$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanSearchSearchGet$Plain(params: {

    /**
     * Search query (Clan Tag and/or Name)
     */
    search: string;

    /**
     * Amount of maximum results to return
     */
    results?: number;
  }): Observable<Array<ClanListingDto>> {

    return this.apiClanSearchSearchGet$Plain$Response(params).pipe(
      map((r: StrictHttpResponse<Array<ClanListingDto>>) => r.body as Array<ClanListingDto>)
    );
  }

  /**
   * Searches all clans relevant to a given search string.
   *
   *
   *
   * This method provides access to the full `HttpResponse`, allowing access to response headers.
   * To access only the response body, use `apiClanSearchSearchGet$Json()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanSearchSearchGet$Json$Response(params: {

    /**
     * Search query (Clan Tag and/or Name)
     */
    search: string;

    /**
     * Amount of maximum results to return
     */
    results?: number;
  }): Observable<StrictHttpResponse<Array<ClanListingDto>>> {

    const rb = new RequestBuilder(this.rootUrl, ClanService.ApiClanSearchSearchGetPath, 'get');
    if (params) {
      rb.path('search', params.search, {});
      rb.query('results', params.results, {});
    }

    return this.http.request(rb.build({
      responseType: 'json',
      accept: 'text/json'
    })).pipe(
      filter((r: any) => r instanceof HttpResponse),
      map((r: HttpResponse<any>) => {
        return r as StrictHttpResponse<Array<ClanListingDto>>;
      })
    );
  }

  /**
   * Searches all clans relevant to a given search string.
   *
   *
   *
   * This method provides access to only to the response body.
   * To access the full response (for headers, for example), `apiClanSearchSearchGet$Json$Response()` instead.
   *
   * This method doesn't expect any request body.
   */
  apiClanSearchSearchGet$Json(params: {

    /**
     * Search query (Clan Tag and/or Name)
     */
    search: string;

    /**
     * Amount of maximum results to return
     */
    results?: number;
  }): Observable<Array<ClanListingDto>> {

    return this.apiClanSearchSearchGet$Json$Response(params).pipe(
      map((r: StrictHttpResponse<Array<ClanListingDto>>) => r.body as Array<ClanListingDto>)
    );
  }

}
