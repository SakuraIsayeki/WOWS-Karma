import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, merge, Subject } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerService } from "../../../services/api/services/player.service";
import { getWowsNumbersPlayerLink } from "../../../services/helpers";
import { mapApiModelState, routeParam, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";


@Component({
  templateUrl: "./profile.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class ProfileComponent {
  // Get the "ID,username" from the route params.
  request$ = routeParam(this.route, "idNamePair")
    .pipe(
      map(idNamePair => parseInt(idNamePair?.split(",")[0]!)),
      mapApiModelState((id) => this.playerService.apiPlayerIdGet$Json({id, includeClanInfo: true})),
      shareReplayRefCount(1),
    );

  profile$ = this.request$.pipe(map(result => result.model));

  // Gets the profile's total karma, as a sum of game + platform karma.
  // Observable profile$ must first successfully emit a profile before calculating the total karma.
  profileTotalKarma$ = this.profile$.pipe(map(profile => (profile?.gameKarma ?? 0) + (profile?.siteKarma ?? 0)));

  currentTabChanged$ = new Subject<"received" | "sent">();
  currentTab$ = merge(
    this.currentTabChanged$,
    this.profile$.pipe(
      map(profile => profile?.optedOut ? "sent" : "received")),
  ).pipe(shareReplayRefCount(1));

  constructor(private route: ActivatedRoute, private playerService: PlayerService) {
  }

  getWowsNumbersPlayerLink(id: number, username: string): string | undefined {
    return getWowsNumbersPlayerLink({id, username});
  }
}
