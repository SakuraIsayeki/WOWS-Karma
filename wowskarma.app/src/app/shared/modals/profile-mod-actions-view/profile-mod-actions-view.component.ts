import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { ModActionType } from 'src/app/services/api/models/mod-action-type';
import { ModActionService } from 'src/app/services/api/services/mod-action.service';
import { InputObservable, shareReplayRefCount, switchMapCatchError, tapAny } from 'src/app/shared/rxjs-operators';

@Component({
  selector: 'profile-mod-actions-view-modal',
  templateUrl: './profile-mod-actions-view.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ProfileModActionsViewComponent {
  @Input()
  @InputObservable()
  public profileId!: number;
  public profileId$!: BehaviorSubject<number>;

  loaded$ = new BehaviorSubject(false);

  @Input() modal!: NgbModalRef;

  modActions$ = this.profileId$.pipe(
    switchMapCatchError((userId) => this.modActionService.apiModActionListGet$Json({ userId })),
    tapAny(() => this.loaded$.next(true)),
    shareReplayRefCount(1),
    map(modActions => modActions ?? []),
  )

  constructor(private modActionService: ModActionService) { }

  static OpenModal(modalService: NgbModal, profile: { id?: number | null }) {
    const modalRef = modalService.open(ProfileModActionsViewComponent, { size: "xl", fullscreen: "xl" });
    modalRef.componentInstance.modal = modalRef;
    modalRef.componentInstance.profileId = profile.id;

    return modalRef;
  }

  countUpdated(modActions: { actionType: ModActionType }[]) {
   return modActions.filter(a => a.actionType === ModActionType.Update).length;
  }

  countDeleted(modActions: { actionType: ModActionType }[]) {
    return modActions.filter(a => a.actionType === ModActionType.Delete).length;
  }
}
