import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { FormBuilder, Validators } from "@angular/forms";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { ModActionType } from "../../../services/api/models";
import { ModActionService } from "../../../services/api/services/mod-action.service";

@Component({
    selector: "app-post-mod-delete",
    templateUrl: "./post-mod-delete.component.html",
    styleUrls: ["./post-mod-delete.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostModDeleteComponent {
    @Input() form = new FormBuilder().nonNullable.group({
        postId: "",
        // actionType: ModActionType.$0,
        reason: ["", [Validators.required, Validators.maxLength(1000)]]
    });
    @Input() modal!: NgbModalRef;

    constructor(private modActionService: ModActionService) {}

    static OpenModal(modalService: NgbModal, post: { id?: string | null }) {
        const modalRef = modalService.open(PostModDeleteComponent, {});
        modalRef.componentInstance.modal = modalRef;
        modalRef.componentInstance.form.controls.postId.patchValue(post.id);
    }

    onSubmit() {
        this.modActionService.apiModActionPost({
            body: {
                postId: this.form.value.postId as string,
                reason: this.form.value.reason as string,
                actionType: ModActionType.Delete,
            },
        }).subscribe(() => {
            this.modal.close();
            window.location.reload();
        });
    }
}
