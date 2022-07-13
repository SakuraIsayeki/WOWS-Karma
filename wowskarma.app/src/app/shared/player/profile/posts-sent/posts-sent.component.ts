import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { BehaviorSubject, combineLatest, combineLatestWith, Observable, tap, merge, filter, withLatestFrom } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { PostsHub } from "../../../../services/hubs/posts-hub.service";
import { filterNotNull, InputObservable, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../rxjs-operators";

@Component({
    selector: "app-posts-sent",
    templateUrl: "./posts-sent.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostsSentComponent {
    @Input()
    @InputObservable()
    userId!: number;
    userId$!: Observable<number>;

    loaded$ = new BehaviorSubject(false);
    shouldRefresh$ = new BehaviorSubject(true); // Set to true to allow initial fetch of posts.

    // Get an observable to fetch received posts on component init.
    sentPosts$ = this.shouldRefresh$.pipe(
        combineLatestWith(this.userId$),
        tap(() => this.loaded$.next(false)),
        filter(([shouldRefresh, userId]) => shouldRefresh && userId != 0),
        switchMapCatchError(p => this.postService.apiPostUserIdSentGet$Json({ userId: p[1] })),
        map(posts => posts?.sort(this.sortByLastCreated)),
        tapAny(() => {
            this.shouldRefresh$.next(false);
            this.loaded$.next(true);
        }),
        shareReplayRefCount(1),
    );

    onchange$ = combineLatest([
        this.userId$,
        merge(this.postsHub.newPost$, this.postsHub.editedPost$, this.postsHub.deletedPost$)
    ]).pipe(
        withLatestFrom(this.sentPosts$),
        filterNotNull(),
        // Map the posts and the ID or Posts's ID of combined events to a new array of posts.
        map(([[userId, p], posts]) => {
            return { posts: (posts as PlayerPostDto[]), userId, post: (p as PlayerPostDto), postId: (p as string) };
        }),
        tap(({ posts, userId, post, postId }) => {
            if (post && post.author?.id === userId || postId && posts.find(p => p.id === postId)) {
                this.shouldRefresh$.next(true);
            }
        }),
    );

    constructor(private postService: PostService, private postsHub: PostsHub) { }

    sortByLastCreated(a: PlayerPostDto, b: PlayerPostDto) {
        return sortByCreationDate(a, b, true);
    }
}
