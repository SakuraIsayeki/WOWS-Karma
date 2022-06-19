import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { map } from "rxjs/operators";
import { getKarmaColor, getWowsNumbersClanLink } from "src/app/services/helpers";
import { ClanListingDto } from "../../../services/api/models/clan-listing-dto";
import { ClanRole } from "../../../services/api/models/clan-role";
import { PlayerProfileDto } from "../../../services/api/models/player-profile-dto";
import { ClanService } from "../../../services/api/services/clan.service";
import { routeParam, shareReplayRefCount, switchMapCatchError } from "../../../shared/rxjs-operators";

@Component({
    templateUrl: "./profile.component.html",
    styleUrls: ["./profile.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProfileComponent {
    // Get the "ID,username" from the route params.
    profile$ = routeParam(this.route, "idNamePair").pipe(
        map(idNamePair => parseInt(idNamePair?.split(",")[0]!)),
        switchMapCatchError((clanId) =>
            this.clanService.apiClanClanIdGet$Json({ clanId }),
        ),
        shareReplayRefCount(1),
    );

    totalMixedKarma$ = this.profile$.pipe(map(clan => clan?.members?.reduce((acc, m) => acc + (m.gameKarma! + m.siteKarma!), 0)!));

    clanCombinedMetrics$ = this.profile$.pipe(
        map(clan => ({
            game: mapMetrics(clan?.members ?? [], m => m.gameKarma!),
            platform: mapMetrics(clan?.members ?? [], m => m.siteKarma!),
            performance: mapMetrics(clan?.members ?? [], m => m.ratingPerformance!),
            teamplay: mapMetrics(clan?.members ?? [], m => m.ratingTeamplay!),
            courtesy: mapMetrics(clan?.members ?? [], m => m.ratingCourtesy!),
        })),
        shareReplayRefCount(1),
    );

    constructor(private route: ActivatedRoute, private clanService: ClanService) { }

    getKarmaColor(karma: number) {
        return getKarmaColor(karma);
    }

    sortByRankThenJoinDate(a: RankAndJoinDateComparisonInput, b: RankAndJoinDateComparisonInput): number {
        return (a.clan!.clanMemberRole! < b.clan!.clanMemberRole!)
            ? -1
            : new Date(a.joinDate!) < new Date(b.joinDate!)
                ? -1
                : 1;
    }

    getWowsNumbersClanLink({ id, tag, name }: ClanListingDto): string | undefined {
        return getWowsNumbersClanLink({ id: id!, tag: tag!, name: name! });
    }
}

type RankAndJoinDateComparisonInput = { clan?: { clanMemberRole?: ClanRole }, joinDate?: string }

type MetricObject = { total: any; min: number; max: number }

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
): MetricObject {
    return {
        total: members.reduce((acc, m) => acc + metricSelector(m), 0),
        min: Math.min(...members.map(metricSelector)),
        max: Math.max(...members.map(metricSelector)),
    };
}