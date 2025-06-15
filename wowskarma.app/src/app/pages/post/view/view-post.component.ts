import { ChangeDetectionStrategy, Component, inject, OnDestroy, signal } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { merge, Subscription, tap, withLatestFrom } from "rxjs";
import { filter, map } from "rxjs/operators";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { ModActionService } from "../../../services/api/services/mod-action.service";
import { PostService } from "../../../services/api/services/post.service";
import { PostsHub } from "../../../services/hubs/posts-hub.service";
import { filterNotNull, mapApiModelState, reloadWhen, routeParam, shareReplayRefCount, switchMapCatchError } from "../../../shared/rxjs-operators";
import { toObservable } from "@angular/core/rxjs-interop";
import { AsyncPipe, NgIf } from "@angular/common";
import { PostComponent } from "../../../shared/post/post.component";
import { ChatLogComponent } from "../../../shared/replay/chat-log/chat-log.component";
import { NotFoundComponent } from "../../fallbacks/not-found/not-found.component";
import { TeamRosterComponent } from "../../../shared/replay/team-roster/team-roster.component";
import { MarkdownComponent } from "ngx-markdown";

@Component({
  templateUrl: "./view-post.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    AsyncPipe,
    PostComponent,
    ChatLogComponent,
    NotFoundComponent,
    TeamRosterComponent,
    NgIf,
    MarkdownComponent
  ]
})
export class ViewPostComponent implements  OnDestroy {
    private route: ActivatedRoute = inject(ActivatedRoute);
    private postService: PostService = inject(PostService);
    private modActionService: ModActionService = inject(ModActionService);
    private postsHub: PostsHub = inject(PostsHub);

    shouldRefresh = signal<null>(null);

    request$ = routeParam(this.route).pipe(
        reloadWhen(toObservable(this.shouldRefresh)),
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
                this.shouldRefresh.set(null);
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

    protected readonly JSON = JSON;
}
