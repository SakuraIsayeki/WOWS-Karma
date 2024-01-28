import {ChangeDetectionStrategy, Component, inject, OnDestroy, OnInit} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { BehaviorSubject, combineLatestWith, debounce, debounceTime, distinct, distinctUntilChanged, merge, Subscription, tap, withLatestFrom } from "rxjs";
import { filter, map } from "rxjs/operators";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { ModActionService } from "../../../services/api/services/mod-action.service";
import { PostService } from "../../../services/api/services/post.service";
import { PostsHub } from "../../../services/hubs/posts-hub.service";
import { filterNotNull, mapApiModelState, reloadWhen, routeParam, shareReplayRefCount, switchMapCatchError, tapAny } from "../../../shared/rxjs-operators";

@Component({
    templateUrl: "./view-post.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewPostComponent implements  OnDestroy {
    private route: ActivatedRoute = inject(ActivatedRoute);
    private postService: PostService = inject(PostService);
    private modActionService: ModActionService = inject(ModActionService);
    private postsHub: PostsHub = inject(PostsHub);

    shouldRefresh$ = new BehaviorSubject<void | null>(null);

    request$ = routeParam(this.route).pipe(
        reloadWhen(this.shouldRefresh$),
        filter((postId) => postId != ""),
        mapApiModelState((id) => this.postService.apiPostPostIdGet$Json({ postId: id! })),
        shareReplayRefCount(1),
    );

    post$ = this.request$.pipe(map(post => post.model));

    onChanges$ = merge(this.postsHub.editedPost$, this.postsHub.deletedPost$).pipe(
        withLatestFrom(this.post$),
        filterNotNull(),
        // Map the posts and the ID or Posts's ID of combined events to a new array of posts.
        map(([p, post]) => {
            return { post, postId: ((p as PlayerPostDto).id ?? (p as string)) };
        }),
        tap(({ post, postId }) => {
            if (post?.id === postId) {
                this.shouldRefresh$.next();
            }
        }),
    );

    lastModAction$ = this.post$.pipe(
        filterNotNull(),
        filter((post) => post.modLocked === true || post.readOnly === true),
        switchMapCatchError((post) => this.modActionService.apiModActionListGet$Json({ postId: post.id! }).pipe(
            filterNotNull(),
        )),
        map((modActions) => modActions![0]),
        shareReplayRefCount(1),
    );

    private onChangesSubscription: Subscription;

    constructor() {
        this.onChangesSubscription = this.onChanges$.subscribe();
    }

    ngOnDestroy() {
        this.onChangesSubscription.unsubscribe();
    }
}
