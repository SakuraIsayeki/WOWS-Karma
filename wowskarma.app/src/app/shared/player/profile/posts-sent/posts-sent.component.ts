import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { BehaviorSubject, combineLatest, combineLatestWith, Observable, tap, merge, filter, withLatestFrom } from "rxjs";
import { distinctUntilChanged, map } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { PostsHub } from "../../../../services/hubs/posts-hub.service";
import { filterNotNull, InputObservable, reloadWhen, shareReplayRefCount, switchMapCatchError, tapAny, tapPageInfoHeaders } from "../../../rxjs-operators";

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

  pageRequest$ = new BehaviorSubject(1);
  pageInfo = new BehaviorSubject<{ currentPage: number, pageSize: number, totalItems: number, totalPages: number } | null>({ currentPage: 1, pageSize: 10, totalItems: 0, totalPages: 0});
  pageInfo$ = this.pageInfo.asObservable().pipe(
    tap(pageInfo => console.debug("pageInfo", pageInfo)),
    filterNotNull(),
    distinctUntilChanged(),
    shareReplayRefCount(1),
  );

  loaded$ = new BehaviorSubject(false);
  shouldRefresh$ = new BehaviorSubject<void | null>(null); // Set to true to allow initial fetch of posts.

  // Get an observable to fetch received posts on component init.
  sentPosts$ = combineLatest([
    this.userId$,
    this.pageRequest$
  ]).pipe(
    tap(() => this.loaded$.next(false)),
    reloadWhen(this.shouldRefresh$),
    filter(([userId,]) => userId != 0 && userId != null),
    switchMapCatchError(([userId, page]) => this.postService.apiPostUserIdSentGet$Json$Response({
      userId,
      page,
      pageSize: 20
    })),
    tapAny(() => this.loaded$.next(true)),
    tapPageInfoHeaders(this.pageInfo),
    map(r => r!.body),
    map(posts => posts?.sort(this.sortByLastCreated)),
    shareReplayRefCount(1)
  );

  onchange$ = combineLatest([
    this.userId$,
    merge(this.postsHub.newPost$, this.postsHub.editedPost$, this.postsHub.deletedPost$)
  ]).pipe(
    withLatestFrom(this.sentPosts$),
    filterNotNull(),
    // Map the posts and the ID or Posts's ID of combined events to a new array of posts.
    map(([[userId, p], posts]) => {
      return {posts: (posts as PlayerPostDto[]), userId, post: (p as PlayerPostDto), postId: (p as string)};
    }),
    tap(({posts, userId, post, postId}) => {
      if (post && post.author?.id === userId || postId && posts.find(p => p.id === postId)) {
        this.shouldRefresh$.next();
      }
    }),
  );

  constructor(private postService: PostService, private postsHub: PostsHub) {
  }

  sortByLastCreated(a: PlayerPostDto, b: PlayerPostDto) {
    return sortByCreationDate(a, b, true);
  }

  onPageChanged(page: number) {
    this.pageRequest$.next(page);
    console.debug("page changed", page);
  }
}
