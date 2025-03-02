import { ChangeDetectionStrategy, Component, inject } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { ClanProfileFullDto } from "src/app/services/api/models/clan-profile-full-dto";
import { ClanRole } from "src/app/services/api/models/clan-role";
import { PlayerProfileDto } from "src/app/services/api/models/player-profile-dto";
import { ClanService } from "src/app/services/api/services/clan.service";
import { MinMaxMetricObject } from "src/app/shared/components/minmax-metric/min-max-metric.component";
import {
  mapApiModelState,
  routeParam,
  shareReplayRefCount,
} from "src/app/shared/rxjs-operators";
import { ColorHexPipe } from "src/app/services/pipes/colorHex.pipe";
import { CommonModule } from "@angular/common";
import { KarmaColorPipe } from "src/app/services/pipes/karma-color.pipe";
import { WowsNumbersClanLinkPipe } from "src/app/services/pipes/wows-numbers-clan-link.pipe";
import { MinMaxMetricComponent } from "src/app/shared/components/minmax-metric/min-max-metric.component";
import { ClanRankComponent } from "src/app/shared/components/icons/clan-rank/clan-rank.component";
import { RouterLink } from "@angular/router";
import { NotFoundComponent } from "../../fallbacks/not-found/not-found.component";
import { MarkdownModule } from "ngx-markdown";

type ApiModelState<T> = {
  notFound?: true;
  model?: T;
};

@Component({
  templateUrl: "./profile.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    ColorHexPipe,
    KarmaColorPipe,
    WowsNumbersClanLinkPipe,
    MinMaxMetricComponent,
    ClanRankComponent,
    RouterLink,
    NotFoundComponent,
    MarkdownModule,
    CommonModule
  ],
})
export class ProfileComponent {
  //loaded$ = new BehaviorSubject<boolean>(false);
  private route: ActivatedRoute = inject(ActivatedRoute);
  private clanService: ClanService = inject(ClanService);

  // Get the "ID,username" from the route params.
  profile$: Observable<ApiModelState<ClanProfileFullDto>> = routeParam(
    this.route,
    "idNamePair",
  ).pipe(
    map((idNamePair) => parseInt(idNamePair?.split(",")[0]!)),
    mapApiModelState((clanId) =>
      this.clanService.apiClanClanIdGet$Json({ clanId }),
    ),
    shareReplayRefCount(1),
    //tapAny(() => this.loaded$.next(true)),
  );

  clan$ = this.profile$.pipe(map((result) => result.model));

  totalMixedKarma$ = this.clan$.pipe(
    map(
      (clan) =>
        clan?.members?.reduce(
          (acc, m) => acc + (m.gameKarma! + m.siteKarma!),
          0,
        )!,
    ),
  );

  clanCombinedMetrics$ = this.clan$.pipe(
    map((clan) => ({
      game: mapMetrics(clan?.members ?? [], (m) => m.gameKarma!),
      platform: mapMetrics(clan?.members ?? [], (m) => m.siteKarma!),
      performance: mapMetrics(clan?.members ?? [], (m) => m.ratingPerformance!),
      teamplay: mapMetrics(clan?.members ?? [], (m) => m.ratingTeamplay!),
      courtesy: mapMetrics(clan?.members ?? [], (m) => m.ratingCourtesy!),
    })),
    shareReplayRefCount(1),
  );

  constructor() {
    // Empty subscription to preemptively load the clan profile ahead of view tree.
    // (uses shareReplayRefCount to prevent multiple requests)
    this.profile$.subscribe();
  }

  sortByRankThenJoinDate(
    a: RankAndJoinDateComparisonInput,
    b: RankAndJoinDateComparisonInput,
  ): number {
    return a.clan!.clanMemberRole! < b.clan!.clanMemberRole!
      ? -1
      : new Date(a.joinDate!) < new Date(b.joinDate!)
        ? -1
        : 1;
  }
}

type RankAndJoinDateComparisonInput = {
  clan?: { clanMemberRole?: ClanRole };
  joinDate?: string;
};

/**
 * Maps the following member metrics to a metric object:
 *
 *  - total: the sum of all member's metrics
 *  - min: the minimum metric held by any member
 *  - max: the maximum metric held by any member
 *
 * @param members The clan members
 * @param metricSelector The metric selected for evaluation
 * @returns The metric object
 */
function mapMetrics(
  members: PlayerProfileDto[],
  metricSelector: (member: PlayerProfileDto) => number,
): MinMaxMetricObject {
  return {
    total: members.reduce((acc, m) => acc + metricSelector(m), 0),
    min: Math.min(...members.map(metricSelector)),
    max: Math.max(...members.map(metricSelector)),
  };
}
