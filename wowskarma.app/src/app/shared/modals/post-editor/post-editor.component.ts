import { ChangeDetectionStrategy, Component, HostListener, Input } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { NgbModal, NgbModalRef } from "@ng-bootstrap/ng-bootstrap";
import { AccountClanListingDto } from "../../../services/api/models/account-clan-listing-dto";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { PostFlairs } from "../../../services/api/models/post-flairs";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { PostService } from "../../../services/api/services/post.service";
import { markTouchedDirtyAndValidate } from "../../../services/helpers";
import { parseFlairsEnum, toEnum } from "../../../services/metricsHelpers";
import * as ReplayValidators from "../../validation/replay-validators";

@Component({
    selector: "post-editor",
    templateUrl: "./post-editor.component.html",
    styleUrls: ["./post-editor.component.scss"],
    changeDetection: ChangeDetectionStrategy.Default,
})
export class PostEditorComponent {
    @Input() post!: PlayerPostEditorDto;
    @Input() modal!: NgbModalRef;

    form = new FormGroup({
        id: new FormControl(""),
        title: new FormControl("", Validators.compose([Validators.required, Validators.minLength(5), Validators.maxLength(60)])),
        content: new FormControl("", Validators.compose([Validators.required, Validators.minLength(50), Validators.maxLength(500)])),
        parsedFlairs: new FormGroup({
            performance: new FormControl<boolean | null>(null),
            teamplay: new FormControl<boolean | null>(null),
            courtesy: new FormControl<boolean | null>(null),
        }),
        replayFile: new FormControl<File | null>(null, ReplayValidators.requireReplay), // [ReplayValidators.requireReplayForNotNullFlairs, ReplayValidators.replayFileValid]),
        guidelinesAccepted: new FormControl(false, Validators.requiredTrue),
    });

    private flairsOptions = [
        { value: false, label: "Negative" },
        { value: null, label: "Neutral" },
        { value: true, label: "Positive" },
    ];

    flairGroups = [
        {
            label: "Performance",
            control: this.form.controls.parsedFlairs.controls.performance,
            options: this.flairsOptions,
        },
        {
            label: "Teamplay",
            control: this.form.controls.parsedFlairs.controls.teamplay,
            options: this.flairsOptions,
        },
        {
            label: "Courtesy",
            control: this.form.controls.parsedFlairs.controls.courtesy,
            options: this.flairsOptions,
        },
    ];


    constructor(private postService: PostService) {
    }

    static OpenEditor(modalService: NgbModal, post: PlayerPostDto) {
        const modalRef = modalService.open(PostEditorComponent, { backdrop: "static", size: "xl", fullscreen: "xl" });
        modalRef.componentInstance.post = PlayerPostEditorDto.fromDto(post);
        modalRef.componentInstance.modal = modalRef;

        // Bind the DTO to the form
        modalRef.componentInstance.form.patchValue(modalRef.componentInstance.post);

        return modalRef;
    }

    @HostListener("change", ["$event.target.files"])
    handleFileInput(fileList: FileList) {
        if (fileList) {
            this.form.controls.replayFile.patchValue(fileList[0]);
            console.debug(this.post.replayFile);
        }
    }

    onSubmit() {

        markTouchedDirtyAndValidate(this.form);
        if (!this.form.valid) {
            return;
        }

        const create = this.post.id === undefined;


        this.post.title = this.form.controls.title.value;
        this.post.content = this.form.controls.content.value;
        const flairs = this?.form.controls.parsedFlairs.value ?? {};
        this.post.flairs = toEnum([flairs.performance!, flairs.teamplay!, flairs.courtesy!]);

        if (this.form.controls.replayFile.value) {
            this.post.replayFile = this.form.controls.replayFile.value;
        }


        if (create) {
            // Create the body as a form data
            const formData = new FormData();
            formData.append("postDto", JSON.stringify(this.post));
            formData.append("replayFile", this.form.controls.replayFile.value!.name);

            this.postService.apiPostPost({
                body: {
                    postDto: JSON.stringify(this.post),
                    replay: this.form.controls.replayFile.value,
                },
            }).subscribe(
                (data) => {
                    console.debug("Created post", data);
                    this.modal.close();
                    window.location.reload();
                },
            );
        } else {
            this.postService.apiPostPut({ body: this.post }).subscribe(
                (data) => {
                    console.debug("Updated post", data);
                    this.modal.close();
                    window.location.reload();
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
        p.parsedFlairs = { performance: flairs[0], teamplay: flairs[1], courtesy: flairs[2] };

        // Initialize missing fields
        p.replayFile = null;
        p.guidelinesAccepted = false;

        return p;
    }
}