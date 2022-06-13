import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { AccountClanListingDto } from "../../../services/api/models/account-clan-listing-dto";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { PostFlairs } from "../../../services/api/models/post-flairs";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { PostService } from "../../../services/api/services/post.service";
import { parseFlairsEnum, toEnum } from "../../../services/metricsHelpers";

@Component({
    selector: "post-editor",
    templateUrl: "./post-editor.component.html",
    styleUrls: ["./post-editor.component.scss"],
    changeDetection: ChangeDetectionStrategy.Default,
})
export class PostEditorComponent {
    @Input() post!: PlayerPostEditorDto;
    @Input() modal!: NgbModalRef;

    constructor(private postService: PostService) { }

    static OpenEditor(modalService: NgbModal, post: PlayerPostDto) {
        const modalRef = modalService.open(PostEditorComponent, { backdrop: "static", size: "xl", fullscreen: "xl" });
        modalRef.componentInstance.post = PlayerPostEditorDto.fromDto(post);
        modalRef.componentInstance.modal = modalRef;
        return modalRef;
    }

    handleFileInput(event: Event) {
        const inputEvent = (event.target as HTMLInputElement);
        if (inputEvent.files) {
            this.post.replayFile = inputEvent.files[0];
            console.debug(this.post.replayFile);
        }
    }

    onSubmit() {
        const create = this.post.id === undefined; // true if new post

        // Map the flairs
        const flairs = this.post.parsedFlairs;
        this.post.flairs = toEnum([flairs.performance, flairs.teamplay, flairs.courtesy]);

        if (create) {
            this.postService.apiPostPost({ body: { postDto: JSON.stringify(this.post) } }).subscribe(
                (data) => {
                    console.debug("Created post", data);
                    this.modal.close();
                },
            );
        } else {
            this.postService.apiPostPut({ body: this.post }).subscribe(
                (data) => {
                    console.debug("Updated post", data);
                    this.modal.close();
                },
            );
        }

        console.debug("Submitting post", this.post);
    }
}

export class PlayerPostEditorDto implements PlayerPostDto {
    author?: AccountClanListingDto;
    content?: null | string;
    createdAt?: null | string;
    flairs?: PostFlairs;
    id?: null | string;
    modLocked?: boolean;
    player?: AccountClanListingDto;
    replay?: ReplayDto;
    replayId?: null | string;
    title?: null | string;
    updatedAt?: null | string;

    parsedFlairs: { performance: boolean | null, teamplay: boolean | null, courtesy: boolean | null } = { performance: null, teamplay: null, courtesy: null };
    replayFile: File | null = null;
    guidelinesAccepted: boolean = false;


    static fromDto(dto: PlayerPostDto): PlayerPostEditorDto {
        let p = dto as PlayerPostEditorDto;
        const flairs = parseFlairsEnum(p.flairs!);
        p.parsedFlairs = { performance: flairs[0], courtesy: flairs[1], teamplay: flairs[2] };

        // Initialize missing fields
        p.replayFile = null;
        p.guidelinesAccepted = false;

        return p;
    }
}