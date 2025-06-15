import { ChangeDetectionStrategy, Component, inject, Input } from '@angular/core';
import { NgbModal } from '@ng-bootstrap/ng-bootstrap';
import { ModActionType, PostModActionDto } from 'src/app/services/api/models';
import { PlayerPostDto } from 'src/app/services/api/models/player-post-dto';
import { ModActionService } from 'src/app/services/api/services/mod-action.service';
import { PostService } from 'src/app/services/api/services/post.service';
import { markTouchedDirtyAndValidate } from 'src/app/services/helpers';
import { parseFlairsEnum, toEnum } from 'src/app/services/metricsHelpers';
import { PlayerPostEditorDto, PostEditorComponent } from 'src/app/shared/modals/post-editor/post-editor.component';
import { NgClass, NgForOf, NgIf } from "@angular/common";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { FormErrorsComponent } from "../../form-errors/form-errors.component";
import { ControlExtensionsDirective } from "../../directives/control-extensions.directive";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'app-post-mod-edit',
  templateUrl: './post-mod-editor.component.html',
  styleUrls: ['./post-mod-editor.component.scss'],
  imports: [
    NgIf,
    FormsModule,
    FormErrorsComponent,
    ReactiveFormsModule,
    ControlExtensionsDirective,
    NgForOf,
    NgClass,
    RouterLink
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostModEditorComponent extends PostEditorComponent {
  @Input() override post: ModPostEditorDto = new ModPostEditorDto();

  private modActionService: ModActionService = inject(ModActionService);
  constructor(postService: PostService) {
    super(postService);
  }

  override onSubmit() {
    markTouchedDirtyAndValidate(this.form);
    if (!this.form.valid) {
      return;
    }

    this.post.title = this.form.controls.title.value;
    this.post.content = this.form.controls.content.value;
    const flairs = this?.form.controls.parsedFlairs.value ?? {};
    this.post.flairs = toEnum([flairs.performance!, flairs.teamplay!, flairs.courtesy!]);

    const modActionDto: PostModActionDto = {
      postId: this.post.id!,
      actionType: ModActionType.Update,
      reason: this.form.value.modReason!,
      updatedPost: this.post,
    }

    this.modActionService.apiModActionPost({ body: modActionDto }).subscribe(
      (data) => {
        console.debug(`Submitted edit mod-action for post ${modActionDto.postId}.`, data);
        this.modal.close();
        window.location.reload();
      },
    );
  }

  static OpenModal(modalService: NgbModal, post: PlayerPostDto) {
    const modalRef = modalService.open(PostModEditorComponent, { backdrop: "static", size: "xl", fullscreen: "xl" });
    modalRef.componentInstance.post = ModPostEditorDto.fromDto(post);
    modalRef.componentInstance.modal = modalRef;

    // Bind the DTO to the form
    modalRef.componentInstance.form.patchValue(modalRef.componentInstance.post);

    return modalRef;
  }
}

export class ModPostEditorDto extends PlayerPostEditorDto {
  modEditReason?: string | null;

  static override fromDto(dto: PlayerPostDto): ModPostEditorDto {
    let p = dto as ModPostEditorDto;
    const flairs = parseFlairsEnum(p.flairs!);
    p.parsedFlairs = {performance: flairs[0], teamplay: flairs[1], courtesy: flairs[2]};

    // Initialize missing fields
    p.replayFile = null;
    p.guidelinesAccepted = false;
    p.modEditReason = null;

    return p;
  }
}
