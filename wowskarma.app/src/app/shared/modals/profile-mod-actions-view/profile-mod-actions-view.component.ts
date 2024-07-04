import { ChangeDetectionStrategy, Component, inject, input, Input, model, signal } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ModActionType } from 'src/app/services/api/models/mod-action-type';
import { ModActionService } from 'src/app/services/api/services/mod-action.service';
import { InputObservable, shareReplayRefCount, switchMapCatchError, tapAny } from 'src/app/shared/rxjs-operators';
import { toObservable } from "@angular/core/rxjs-interop";
import { PostModActionDto } from "../../../services/api/models/post-mod-action-dto";

@Component({
  selector: 'profile-mod-actions-view-modal',
  templateUrl: './profile-mod-actions-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileModActionsViewComponent {
  profileId = model<number>()
  modActions = model<PostModActionDto[]>()

  loaded = signal(false);

  @Input() modal!: NgbModalRef;

  modActionService = inject(ModActionService)

  modActions$= toObservable(this.profileId).pipe(
    switchMapCatchError((userId) => this.modActionService.apiModActionListGet$Json({ userId })),
    tapAny(() => this.loaded.set(true)),
    shareReplayRefCount(1),
    map(modActions => modActions ?? []),
  )

  static OpenModal(modalService: NgbModal, modActions?: PostModActionDto[], profileId?: number) {
    const modalRef = modalService.open(ProfileModActionsViewComponent, { size: "xl", fullscreen: "xl" });
    modalRef.componentInstance.modal = modalRef;
    modalRef.componentInstance.modActions.set(modActions);
    modalRef.componentInstance.profileId.set(profileId);
    console.debug(modalRef.componentInstance.profileId())


    return modalRef;
  }

  countUpdated(modActions: { actionType: ModActionType }[]) {
   return modActions.filter(a => a.actionType === ModActionType.Update).length;
  }

  countDeleted(modActions: { actionType: ModActionType }[]) {
    return modActions.filter(a => a.actionType === ModActionType.Delete).length;
  }
}
