import {ChangeDetectionStrategy, Component, inject} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, combineLatest, merge, of, Subject, tap } from "rxjs";
import { filter, map } from "rxjs/operators";
import { ModActionService } from 'src/app/services/api/services/mod-action.service';
import { PlatformBansService } from 'src/app/services/api/services/platform-bans.service';
import { AuthService } from 'src/app/services/auth.service';
import { ProfileModActionsViewComponent } from 'src/app/shared/modals/profile-mod-actions-view/profile-mod-actions-view.component';
import { ProfilePlatformBansViewComponent } from 'src/app/shared/modals/profile-platform-bans-view/profile-platform-bans-view.component';
import { PlayerService } from "../../../services/api/services/player.service";
import { getWowsNumbersPlayerLink } from "../../../services/helpers";
import { filterNotNull, mapApiModelState, routeParam, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";
import { ProfileService } from "../../../services/api/services/profile.service";


@Component({
  templateUrl: "./profile.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})

export class ProfileComponent {
  // Get the "ID,username" from the route params.
  private route: ActivatedRoute = inject(ActivatedRoute);
  private playerService: PlayerService = inject(PlayerService);
  private profileService: ProfileService = inject(ProfileService);
  public authService: AuthService = inject(AuthService);
  private modActionsService: ModActionService = inject(ModActionService);
  private platformBansService: PlatformBansService = inject(PlatformBansService);
  private modalService: NgbModal = inject(NgbModal);

  request$ = routeParam(this.route, "idNamePair")
    .pipe(
      map(idNamePair => parseInt(idNamePair?.split(",")[0]!)),
      mapApiModelState((id) => this.playerService.apiPlayerIdGet$Json({id, includeClanInfo: true})),
      shareReplayRefCount(1),
    );

  profile$ = this.request$.pipe(map(result => result.model));

  platformBans$ = this.profile$.pipe(
    // Only send a request if the user is a CM and the profile is loaded.
    filter(() => this.isCurrentUserCM),
    switchMapCatchError((profile) => this.platformBansService.apiModBansUserIdGet$Json({userId: profile!.id!})),
    shareReplayRefCount(1)
  )

  isPlatformBanned$ = this.platformBans$.pipe(
    map(bans => (bans && bans.length > 0) ?? false),
    shareReplayRefCount(1)
  );

  profileModActions$ = this.profile$.pipe(
    filter(() => this.isCurrentUserCM),
    switchMapCatchError((profile) => this.modActionsService.apiModActionListGet$Json({userId: profile!.id!})),
    shareReplayRefCount(1)
  );

  profileRoles$ = this.request$.pipe(
    map(result => this.profileService.apiProfileIdGet$Json({id: result.model!.id!})),
    switchMapCatchError((profile) => profile),
    map(profile => profile?.profileRoles ?? []),
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

  get isCurrentUserCM() {
    return this.authService.isInRole('mod');
  }

  get openPlatformBansModal() {
    return this.profile$.subscribe(profile => ProfilePlatformBansViewComponent.OpenModal(this.modalService, profile!));
  }

  get openModActionsModal() {
    return this.profile$.subscribe(profile => ProfileModActionsViewComponent.OpenModal(this.modalService, profile!));
  }
}
