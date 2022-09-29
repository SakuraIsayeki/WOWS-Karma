import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, combineLatest, merge, of, Subject, tap } from "rxjs";
import { filter, map } from "rxjs/operators";
import { PlatformBansService } from 'src/app/services/api/services/platform-bans.service';
import { AuthService } from 'src/app/services/auth.service';
import { PlayerService } from "../../../services/api/services/player.service";
import { getWowsNumbersPlayerLink } from "../../../services/helpers";
import { filterNotNull, mapApiModelState, routeParam, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";


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

  get isCurrentUserCM() {
    return this.authService.isInRole('mod');
  }

  platformBans$ = this.profile$.pipe(
    tap(() => console.debug("Fetching platform bans...")),
    // Only send a request if the user is a CM and the profile is loaded.
    filter(() => this.isCurrentUserCM),
    tap(() => console.debug("User is a CM, fetching platform bans...")),
    switchMapCatchError((profile) => this.platformBansService.apiModBansUserIdGet$Json({userId: profile!.id!})),
    tapAny(() => console.debug("Fetched platform bans.")),
    shareReplayRefCount(1)
  )

  isPlatformBanned$ = this.platformBans$.pipe(
    map(bans => (bans && bans.length > 0) ?? false),
    shareReplayRefCount(1)
  );

  // Gets the profile's total karma, as a sum of game + platform karma.
  // Observable profile$ must first successfully emit a profile before calculating the total karma.
  profileTotalKarma$ = this.profile$.pipe(map(profile => (profile?.gameKarma ?? 0) + (profile?.siteKarma ?? 0)));

  currentTabChanged$ = new Subject<"received" | "sent">();
  currentTab$ = merge(
    this.currentTabChanged$,
    this.profile$.pipe(
      map(profile => profile?.optedOut ? "sent" : "received")),
  ).pipe(shareReplayRefCount(1));

  constructor(private route: ActivatedRoute, private playerService: PlayerService, public authService: AuthService, private platformBansService: PlatformBansService) {
  }

  getWowsNumbersPlayerLink(id: number, username: string): string | undefined {
    return getWowsNumbersPlayerLink({id, username});
  }
}
