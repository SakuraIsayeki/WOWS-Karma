import { ChangeDetectionStrategy, Component, OnDestroy, OnInit } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, combineLatestWith, debounce, debounceTime, distinct, distinctUntilChanged, merge, tap, withLatestFrom } from "rxjs";
import { filter, map } from "rxjs/operators";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { ModActionService } from "../../../services/api/services/mod-action.service";
import { PostService } from "../../../services/api/services/post.service";
import { PostsHub } from "../../../services/hubs/posts-hub.service";
import { filterNotNull, routeParam, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";

@Component({
    templateUrl: "./view-post.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewPostComponent {
    shouldRefresh$ = new BehaviorSubject(true);

    post$ = this.shouldRefresh$.pipe(
        combineLatestWith(routeParam(this.route)),
        filter(([shouldRefresh, postId]) => shouldRefresh && postId != ""),
        map(([, postId]) => postId),
        switchMapCatchError((id) => this.postService.apiPostPostIdGet$Json({ postId: id! })),
        tap(() => {
            this.shouldRefresh$.next(false);
        }),
        shareReplayRefCount(1),
    );

    onChanges$ = merge(this.postsHub.editedPost$, this.postsHub.deletedPost$).pipe(
        withLatestFrom(this.post$),
        filterNotNull(),
        // Map the posts and the ID or Posts's ID of combined events to a new array of posts.
        map(([p, post]) => {
            return { post, postId: ((p as PlayerPostDto).id ?? (p as string)) };
        }),
        tap(({ post, postId }) => {
            if (post?.id === postId) {
                this.shouldRefresh$.next(true);
            }
        }),
    );

    lastModAction$ = this.post$.pipe(
        filterNotNull(),
        filter((post) => post.modLocked === true),
        switchMapCatchError((post) => this.modActionService.apiModActionListGet$Json({ postId: post.id! }).pipe(
            filterNotNull(),
        )),
        map((modActions) => modActions![0]),
        shareReplayRefCount(1),
    );

    constructor(private route: ActivatedRoute, private postService: PostService, private modActionService: ModActionService, private postsHub: PostsHub) {

    }
}
