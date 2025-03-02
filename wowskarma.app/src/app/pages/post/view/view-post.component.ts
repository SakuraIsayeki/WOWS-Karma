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
import { PostComponent } from "src/app/shared/post/post.component";
import { AsyncPipe, CommonModule, NgIf } from "@angular/common";
import { ReactiveFormsModule } from "@angular/forms";
import { MarkdownComponent } from "ngx-markdown";
import { NotFoundComponent } from "../../fallbacks/not-found/not-found.component";
import { TeamRosterComponent } from "src/app/shared/replay/team-roster/team-roster.component";
import { ChatLogComponent } from "src/app/shared/replay/chat-log/chat-log.component";

@Component({
    templateUrl: "./view-post.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        PostComponent,
        AsyncPipe,
        NgIf,
        CommonModule,
        ReactiveFormsModule,
        MarkdownComponent,
        NotFoundComponent,
        TeamRosterComponent,
        ChatLogComponent
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
