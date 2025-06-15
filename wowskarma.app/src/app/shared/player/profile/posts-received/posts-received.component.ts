import { ChangeDetectionStrategy, Component, inject, input, signal, TemplateRef, ViewChild } from "@angular/core";
import { NgbModal, NgbPagination } from "@ng-bootstrap/ng-bootstrap";
import { BehaviorSubject, combineLatest, combineLatestWith, filter, firstValueFrom, merge, Observable, shareReplay, tap, withLatestFrom } from "rxjs";
import { distinctUntilChanged, map, startWith, } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { AuthService } from "../../../../services/auth.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { PostsHub } from "../../../../services/hubs/posts-hub.service";
import { PostEditorComponent } from "../../../modals/post-editor/post-editor.component";
import { filterNotNull, InputObservable, reloadWhen, shareReplayRefCount, switchMapCatchError, tapAny, tapPageInfoHeaders } from "../../../rxjs-operators";
import { toObservable } from "@angular/core/rxjs-interop";
import { AsyncPipe, DatePipe, NgIf, NgTemplateOutlet } from "@angular/common";
import { PostComponent } from "../../../post/post.component";
import { RouterLink } from "@angular/router";


@Component({
  selector: "app-posts-received",
  templateUrl: "./posts-received.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    AsyncPipe,
    NgTemplateOutlet,
    NgIf,
    NgbPagination,
    PostComponent,
    RouterLink,
    DatePipe
  ]
})
export class PostsReceivedComponent {
  @ViewChild("createPostButton") buttonTemplate!: TemplateRef<any>;
  @ViewChild("selfPost") selfPostTemplate!: TemplateRef<any>;
  @ViewChild("notAuthenticated") notAuthenticatedTemplate!: TemplateRef<any>;
  @ViewChild("optedOut") optedOutTemplate!: TemplateRef<any>;
  @ViewChild("platformBanned") bannedTemplate!: TemplateRef<any>;
  @ViewChild("postsCooldown") cooldownTemplate!: TemplateRef<any>;

  public authService: AuthService = inject(AuthService);
  private postService: PostService = inject(PostService);
  private modalService: NgbModal = inject(NgbModal);
  public postsHub: PostsHub = inject(PostsHub);

  pageRequest$ = new BehaviorSubject(1);
  pageInfo = new BehaviorSubject<{ currentPage: number, pageSize: number, totalItems: number, totalPages: number } | null>({ currentPage: 1, pageSize: 10, totalItems: 0, totalPages: 0});
  pageInfo$ = this.pageInfo.asObservable().pipe(
    tap(pageInfo => console.debug("pageInfo", pageInfo)),
    filterNotNull(),
    distinctUntilChanged(),
    shareReplayRefCount(1),
  );

  userId = input<number>(0);

  loaded = signal(false);
  shouldRefresh = signal(null); // Set to true to allow initial fetch of posts.

  // Get an observable to fetch received posts on component init, and refresh on hub events.
  receivedPosts$ = combineLatest([
    toObservable(this.userId),
    this.pageRequest$
  ]).pipe(
    tap(() => this.loaded.set(false)),
    reloadWhen(toObservable(this.shouldRefresh)),
    filter(([userId,]) => userId != 0),
    switchMapCatchError(([userId, page]) => this.postService.apiPostUserIdReceivedGet$Json$Response({
      userId,
      page,
      pageSize: 20,
    })),
    tapAny(() => this.loaded.set(true)),
    tapPageInfoHeaders(this.pageInfo),
    map(r => r!.body),
    map(posts => posts?.sort(this.sortByLastCreated)),
    shareReplayRefCount(1)
  );

  userCooldownStatus$ = this.authService.userInfo$.pipe(
    combineLatestWith(this.receivedPosts$),
    map(([userInfo, posts]) => {
      // Get all posts made by current user, ordered by creation date (newest first).
      const currentUserPosts = posts.filter(p => p.author?.id == userInfo?.id)?.sort(this.sortByLastCreated);

      if (currentUserPosts?.length != 0) {
        // Get the last post made by current user.
        const lastPost = currentUserPosts![0];
        // Get the post's creation date, add 24 hours to it, check if it's in the future.
        const lastPostCreationDate = new Date(lastPost.createdAt!);
        const in24hrs = new Date(lastPostCreationDate.getTime() + 24 * 60 * 60 * 1000);

        return {status: in24hrs > new Date(Date.now()), cooldown: in24hrs};
      }

      return {status: false, cooldown: null};
    }));

  showState$: Observable<{ template: TemplateRef<any>, context?: any }> = combineLatest([
    this.authService.userInfo$,
    this.authService.profileFlags$,
    this.userCooldownStatus$,
  ]).pipe(map(([userInfo, profileFlags, cooldown]) => {
    // Check for user authentication.
    if (!userInfo) {
      return {template: this.notAuthenticatedTemplate};
    }

    // Check for user opted out.
    if (profileFlags?.optedOut) {
      return {template: this.optedOutTemplate};
    }

    // Check for user platform ban.
    if (profileFlags?.postsBanned) {
      return {template: this.bannedTemplate};
    }

    // Check if same user.
    if (this.userId() == userInfo?.id) {
      return {template: this.selfPostTemplate};
    }

    // Check for posts cooldown.
    if (cooldown.status === true) {
      return {template: this.cooldownTemplate, context: {until: cooldown.cooldown}};
    }

    return {template: this.buttonTemplate};
  }));

  constructor() {
    combineLatest([
      toObservable(this.userId),
      merge(this.postsHub.newPost$, this.postsHub.editedPost$, this.postsHub.deletedPost$)
    ]).pipe(
      withLatestFrom(this.receivedPosts$),
      filterNotNull(),
      // Map the posts and the ID or Posts's ID of combined events to a new array of posts.
      map(([[userId, p], posts]) => {
        return {posts: (posts as PlayerPostDto[]), userId, post: (p as PlayerPostDto), postId: (p as string)};
      }),
      tap(({posts, userId, post, postId}) => {
        if (post && post.player?.id === userId || postId && posts.find(p => p.id === postId)) {
          this.shouldRefresh.set(null);
        }
      }),
    ).subscribe();
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

  async openEditor() {
    const modal = PostEditorComponent.OpenEditor(this.modalService, {});
    modal.componentInstance.post.player = { id: this.userId() }
    modal.componentInstance.post.author = { id: (await firstValueFrom(this.authService.userInfo$))?.id };
  }

  getCurrentLocation() {
    return window.location.pathname;
  }

  onPageChanged(page: number) {
    this.pageRequest$.next(page);
    console.debug("page changed", page);
  }
}
