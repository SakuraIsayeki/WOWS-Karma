import { ChangeDetectionStrategy, Component, EventEmitter, Input, Output } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { PlayerPostDto } from "../../services/api/models/player-post-dto";
import { AuthService } from "../../services/auth.service";
import { getPostBorderColor } from "../../services/helpers";
import { PostEditorComponent } from "../modals/create-post/post-editor.component";

@Component({
    selector: "app-post",
    templateUrl: "./post.component.html",
    styleUrls: ["./post.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    inputs: ["post", "postDisplayType"],

})

//TODO: Implement editor modals

export class PostComponent {
    @Input() public post?: PlayerPostDto;
    @Input() public postDisplayType?: "neutral" | "received" | "sent";

    constructor(public authService: AuthService, private modalService: NgbModal) { }

    get canEdit() {
        return !this.post?.modLocked
            && this.authService.userInfo$.value?.id === this.post?.author?.id;
    }

    getPostBorderColor({ flairs }: PlayerPostDto) {
        return getPostBorderColor({ flairs });
    }

    openEditor() {
        const modalRef = PostEditorComponent.OpenEditor(this.modalService, this.post!);
        modalRef.componentInstance.post = this.post;
        return modalRef;
    }
}
