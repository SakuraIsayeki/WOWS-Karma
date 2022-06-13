import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { NgbModal } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject, combineLatestWith, Observable, shareReplay, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { AuthService } from "../../../../services/auth.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { PostEditorComponent } from "../../../modals/create-post/post-editor.component";
import { InputObservable, switchMapCatchError, tapAny } from "../../../rxjs-operators";

@Component({
    selector: "app-posts-received",
    templateUrl: "./posts-received.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})

export class PostsReceivedComponent {
    @Input()
    @InputObservable()
    userId!: number;
    userId$!: Observable<number>;

    loaded$ = new BehaviorSubject(false);

    // Get an observable to fetch received posts on component init.
    receivedPosts$ = this.userId$.pipe(
        tap(() => this.loaded$.next(false)),
        switchMapCatchError(userId => this.postService.apiPostUserIdReceivedGet$Json({ userId })),
        tapAny(() => this.loaded$.next(true)),
        shareReplay(1),
    );

    constructor(private postService: PostService, public authService: AuthService, private modalService: NgbModal) { }

    get isAuthenticated() {
        return this.authService.isAuthenticated;
    }

    get isUserPlatformBanned$() {
        return this.authService.profileFlags$.pipe(
            map(profileFlags => profileFlags?.postsBanned ?? false),
            shareReplay(1),
        );
    }

    get isSameUser$() {
        return this.authService.userInfo$.pipe(
            map(userInfo => userInfo?.id !== null && userInfo?.id === this.userId),
        );
    }

    get isUserOnCooldown$() {
        return this.authService.userInfo$.pipe(
            combineLatestWith(this.receivedPosts$),
            map(([userInfo, posts]) => {
                // Get all posts made by current user, ordered by creation date (newest first).
                const currentUserPosts = posts?.filter(p => p.author?.id == userInfo?.id)?.sort(this.sortByLastCreated);

                if (currentUserPosts?.length != 0) {
                    // Get the last post made by current user.
                    const lastPost = currentUserPosts![currentUserPosts!.length - 1];
                    // Get the post's creation date, add 24 hours to it, check if it's in the future.
                    const lastPostCreationDate = new Date(lastPost.createdAt!);
                    const in24hrs = new Date(lastPostCreationDate.getTime() + 24 * 60 * 60 * 1000);

                    return { status: in24hrs > new Date(Date.now()), cooldown: in24hrs };
                }

                return { status: false, cooldown: null };
            }));
    }

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
        PostEditorComponent.OpenEditor(this.modalService, {});
    }

    getCurrentLocation() {
        return window.location.pathname;
    }
}
