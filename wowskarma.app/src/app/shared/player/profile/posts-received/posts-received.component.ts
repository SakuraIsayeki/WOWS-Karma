import { ChangeDetectionStrategy, Component, Input, TemplateRef, ViewChild } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject, combineLatest, combineLatestWith, Observable, shareReplay, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { AuthService } from "../../../../services/auth.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { PostEditorComponent } from "../../../modals/post-editor/post-editor.component";
import { InputObservable, switchMapCatchError, tapAny } from "../../../rxjs-operators";


@Component({
    selector: "app-posts-received",
    templateUrl: "./posts-received.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostsReceivedComponent {
    @ViewChild("createPostButton")
    buttonTemplate!: TemplateRef<any>;

    @ViewChild("selfPost")
    selfPostTemplate!: TemplateRef<any>;

    @ViewChild("notAuthenticated")
    notAuthenticatedTemplate!: TemplateRef<any>;

    @ViewChild("optedOut")
    optedOutTemplate!: TemplateRef<any>;

    @ViewChild("platformBanned")
    bannedTemplate!: TemplateRef<any>;

    @ViewChild("postsCooldown")
    cooldownTemplate!: TemplateRef<any>;


    @Input()
    @InputObservable()
    userId!: number;
    userId$!: Observable<number>;

    loaded$ = new BehaviorSubject(false);

    // Get an observable to fetch received posts on component init.
    receivedPosts$ = this.userId$.pipe(
        tap(() => this.loaded$.next(false)),
        switchMapCatchError(userId => this.postService.apiPostUserIdReceivedGet$Json({ userId })),
        map(posts => posts?.sort(this.sortByLastCreated)),
        tapAny(() => this.loaded$.next(true)),
        shareReplay(1),
    );

    userCooldownStatus$ = this.authService.userInfo$.pipe(
        combineLatestWith(this.receivedPosts$),
        map(([userInfo, posts]) => {
            // Get all posts made by current user, ordered by creation date (newest first).
            const currentUserPosts = posts?.filter(p => p.author?.id == userInfo?.id)?.sort(this.sortByLastCreated)

            if (currentUserPosts?.length != 0) {
                // Get the last post made by current user.
                const lastPost = currentUserPosts![0];
                // Get the post's creation date, add 24 hours to it, check if it's in the future.
                const lastPostCreationDate = new Date(lastPost.createdAt!);
                const in24hrs = new Date(lastPostCreationDate.getTime() + 24 * 60 * 60 * 1000);

                return { status: in24hrs > new Date(Date.now()), cooldown: in24hrs };
            }

            return { status: false, cooldown: null };
        }));

    showState$: Observable<{ template: TemplateRef<any>, context?: any }> = combineLatest([
        this.authService.userInfo$,
        this.authService.profileFlags$,
        this.userCooldownStatus$,
    ]).pipe(map(([userInfo, profileFlags, cooldown]) => {
        // Check for user authentication.
        if (!userInfo) {
            return { template: this.notAuthenticatedTemplate };
        }

        // Check for user opted out.
        if (profileFlags?.optedOut) {
            return { template: this.optedOutTemplate };
        }

        // Check for user platform ban.
        if (profileFlags?.postsBanned) {
            return { template: this.bannedTemplate };
        }

        // Check if same user.
        if (this.userId == userInfo?.id) {
            return { template: this.selfPostTemplate };
        }

        // Check for posts cooldown.
        if (cooldown.status === true) {
            return { template: this.cooldownTemplate, context: { until: cooldown.cooldown } };
        }

        return { template: this.buttonTemplate };
    }));

    constructor(private postService: PostService, public authService: AuthService, private modalService: NgbModal) { }

    get hasUserOptedOut$() {
        return this.authService.profileFlags$.pipe(
            map(profileFlags => profileFlags?.optedOut ?? false),
            shareReplay(1),
        );
    }

    sortByLastCreated(a: PlayerPostDto, b: PlayerPostDto) {
        return sortByCreationDate(a, b, true);
    }

    openEditor() {
        const modal = PostEditorComponent.OpenEditor(this.modalService, {});
        combineLatest([this.userId$, this.authService.userInfo$]).subscribe(([userId, userInfo]) => {
            modal.componentInstance.post.player = { id: userId };
            modal.componentInstance.post.author = { id: userInfo?.id };
        });
    }

    getCurrentLocation() {
        return window.location.pathname;
    }
}
