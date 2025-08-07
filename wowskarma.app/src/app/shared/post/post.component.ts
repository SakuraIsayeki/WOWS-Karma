import { ChangeDetectionStrategy, Component, computed, input, inject } from "@angular/core";
import { NgbModal, NgbTooltip } from "@ng-bootstrap/ng-bootstrap";
import { PostModEditorComponent } from 'src/app/shared/modals/post-mod-edit/post-mod-editor.component';
import { PlayerPostDto } from "../../services/api/models/player-post-dto";
import { AuthService } from "../../services/auth.service";
import { PostDeleteComponent } from "../modals/post-delete/post-delete.component";
import { PostEditorComponent } from "../modals/post-editor/post-editor.component";
import { PostModDeleteComponent } from "../modals/post-mod-delete/post-mod-delete.component";
import { NgIf, DatePipe } from "@angular/common";
import { PlayerNamelinkComponent } from "../components/player-namelink/player-namelink.component";
import { MarkdownComponent } from "ngx-markdown";
import { FlairMarkupsComponent } from "./flair-markup/flair-markups.component";
import { RouterLink } from "@angular/router";
import { PostBorderColorPipe } from "../../services/pipes/post-border-color.pipe";

@Component({
    selector: "app-post",
    templateUrl: "./post.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [NgIf, PlayerNamelinkComponent, MarkdownComponent, FlairMarkupsComponent, RouterLink, NgbTooltip, DatePipe, PostBorderColorPipe]
})

export class PostComponent {
  authService = inject(AuthService);
  private modalService = inject(NgbModal);

  public post = input<PlayerPostDto>();
  public postDisplayType = input.required<"neutral" | "received" | "sent">();
  public isOwnerOrPrivileged = computed(() =>
    this.authService.userInfo$.value?.id === this.post()?.author?.id
    || this.authService.userInfo$.value?.roles?.includes("mod")
    || this.authService.userInfo$.value?.roles?.includes("admin")
    || this.authService.userInfo$.value?.roles?.includes("wg")
  );

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {
  }

  get canDelete() {
    return !this.post()?.modLocked
      && this.authService.userInfo$.value?.id === this.post()?.author?.id;
  }

  get canEdit() {
    return this.canDelete && !this.post()?.readOnly;
  }

  openEditor() {
    return PostEditorComponent.OpenEditor(this.modalService, this.post()!);
  }

  openModEditor() {
    return PostModEditorComponent.OpenModal(this.modalService, this.post()!);
  }

  openDeleteModal() {
    return PostDeleteComponent.OpenModal(this.modalService, this.post()!);
  }

  openModDeleteModal() {
    return PostModDeleteComponent.OpenModal(this.modalService, this.post()!);
  }
}
