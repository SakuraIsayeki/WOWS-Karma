import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { PlayerPostDto } from "../../services/api/models/player-post-dto";
import { AuthService } from "../../services/auth.service";
import { getPostBorderColor } from "../../services/helpers";
import { PostDeleteComponent } from "../modals/post-delete/post-delete.component";
import { PostEditorComponent } from "../modals/post-editor/post-editor.component";
import { PostModDeleteComponent } from "../modals/post-mod-delete/post-mod-delete.component";

@Component({
    selector: "app-post",
    templateUrl: "./post.component.html",
    styleUrls: ["./post.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    inputs: ["post", "postDisplayType"],

})

export class PostComponent {
    @Input() public post?: PlayerPostDto;
    @Input() public postDisplayType?: "neutral" | "received" | "sent";

    constructor(public authService: AuthService, private modalService: NgbModal) { }

    get canEdit() {
        return !this.post?.modLocked
            && this.authService.userInfo$.value?.id === this.post?.author?.id;
    }

    openEditor() {
        return PostEditorComponent.OpenEditor(this.modalService, this.post!);
    }

    openDeleteModal() {
        return PostDeleteComponent.OpenModal(this.modalService, this.post!);
    }

    openModDeleteModal() {
        return PostModDeleteComponent.OpenModal(this.modalService, this.post!);
    }
}
