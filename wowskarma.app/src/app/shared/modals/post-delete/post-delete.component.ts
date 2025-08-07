import { ChangeDetectionStrategy, Component, Input, inject } from "@angular/core";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { PostService } from "../../../services/api/services/post.service";

@Component({
    selector: 'app-post-delete',
    templateUrl: './post-delete.component.html',
    styleUrls: ['./post-delete.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true
})
export class PostDeleteComponent {
  private postService = inject(PostService);

  @Input() public postId!: number;
  @Input() modal!: NgbModalRef;

  /** Inserted by Angular inject() migration for backwards compatibility */
  constructor(...args: unknown[]);

  constructor() {}

  static OpenModal(modalService: NgbModal, post: { id?: string | null }) {
    const modalRef = modalService.open(PostDeleteComponent, {});
    modalRef.componentInstance.modal = modalRef;
    modalRef.componentInstance.postId = post.id;
  }

  onSubmit() {
    this.postService.apiPostPostIdDelete$Response({ postId: this.postId.toString() }).subscribe(() => {
        this.modal.close();
        window.location.reload();
    });
  }
}
