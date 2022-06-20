import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services/player.service";
import { getKarmaColor, getWowsNumbersPlayerLink } from "../../../services/helpers";
import { routeParam, shareReplayRefCount, switchMapCatchError } from "../../../shared/rxjs-operators";


@Component({
    templateUrl: "./profile.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})

export class ProfileComponent {
    // Get the "ID,username" from the route params.
    profile$ = routeParam(this.route, "idNamePair")
        .pipe(
            map(idNamePair => parseInt(idNamePair?.split(",")[0]!)),
            switchMapCatchError((id) =>
                this.playerService.apiPlayerIdGet$Json({ id, includeClanInfo: true })),
            shareReplayRefCount(1));

    // Gets the profile's total karma, as a sum of game + platform karma.
    // Observable profile$ must first successfully emit a profile before calculating the total karma.
    profileTotalKarma$ = this.profile$.pipe(map(profile => (profile?.gameKarma ?? 0) + (profile?.siteKarma ?? 0)));

    currentTab = "received";

    constructor(private route: ActivatedRoute, private playerService: PlayerService) {
        this.profile$.subscribe(profile => this.currentTab = profile?.optedOut ? "sent" : "received");
    }

    getKarmaColor(karma: number) {
        return getKarmaColor(karma);
    }

    getWowsNumbersPlayerLink(id: number, username: string): string | undefined {
        return getWowsNumbersPlayerLink({ id, username });
    }
}
