import { ChangeDetectionStrategy, Component, inject, input, Input, model, signal } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject, of, pipe } from 'rxjs';
import { map } from 'rxjs/operators';
import { PlatformBansService } from 'src/app/services/api/services/platform-bans.service';
import {
  filterNotNull,
  InputObservable,
  shareReplayRefCount,
  switchMapCatchError,
  tapAny
} from 'src/app/shared/rxjs-operators';
import { PlatformBanDto } from "../../../services/api/models/platform-ban-dto";
import { toObservable } from "@angular/core/rxjs-interop";

@Component({
  templateUrl: './profile-platform-bans-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfilePlatformBansViewComponent {
  profileId = model<number>()
  platformBans = model<PlatformBanDto[]>()

  @Input() modal!: NgbModalRef;

  loaded = signal(false);

  platformBansService = inject(PlatformBansService);

  platformBans$ = toObservable(this.profileId).pipe(
    filterNotNull(),
    switchMapCatchError((userId) => this.platformBansService.apiModBansUserIdGet$Json({ userId })),
    tapAny(() => this.loaded.set(true)),
    shareReplayRefCount(1),
    map(bans => bans ?? []),
  )

  static OpenModal(modalService: NgbModal, platformBans?: PlatformBanDto[], profileId?: number) {
    const modalRef = modalService.open(ProfilePlatformBansViewComponent, { size: "lg", fullscreen: "lg" });
    modalRef.componentInstance.modal = modalRef;
    modalRef.componentInstance.platformBans.set(platformBans);
    modalRef.componentInstance.profileId.set(profileId);

    return modalRef;
  }

  filterReverted(bans: { reverted?: boolean }[]) {
    return bans.filter(b => !b.reverted);
  }
}
