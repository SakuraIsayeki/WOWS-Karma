import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, of, pipe } from 'rxjs';
import { map } from 'rxjs/operators';
import { PlatformBansService } from 'src/app/services/api/services/platform-bans.service';
import { InputObservable, shareReplayRefCount, switchMapCatchError, tapAny } from 'src/app/shared/rxjs-operators';

@Component({
  templateUrl: './profile-platform-bans-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfilePlatformBansViewComponent {
  @Input()
  @InputObservable()
  public profileId!: number;
  public profileId$!: BehaviorSubject<number>;

  @Input() modal!: NgbModalRef;

  loaded$ = new BehaviorSubject(false);

  platformBans$ = this.profileId$.pipe(
    switchMapCatchError((userId) => this.platformBansService.apiModBansUserIdGet$Json({ userId })),
    tapAny(() => this.loaded$.next(true)),
    shareReplayRefCount(1),
    map(bans => bans ?? []),
  )

  constructor(private platformBansService: PlatformBansService) { }

  static OpenModal(modalService: NgbModal, profile: { id?: number | null }) {
    const modalRef = modalService.open(ProfilePlatformBansViewComponent, { size: "lg", fullscreen: "lg" });
    modalRef.componentInstance.modal = modalRef;
    modalRef.componentInstance.profileId = profile.id;

    return modalRef;
  }

  filterReverted(bans: { reverted?: boolean }[]) {
    return bans.filter(b => !b.reverted);
  }
}
