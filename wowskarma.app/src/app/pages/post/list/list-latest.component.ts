import {ChangeDetectionStrategy, Component, inject} from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { BehaviorSubject, combineLatest, Observable, of } from "rxjs";
import { map, debounceTime, distinctUntilChanged, shareReplay, startWith, tap } from "rxjs/operators";
import { PlayerPostDto } from "src/app/services/api/models/player-post-dto";
import { PostService } from "src/app/services/api/services/post.service";
import { AuthService } from "src/app/services/auth.service";
import { PostsHub } from "src/app/services/hubs/posts-hub.service";
import { filterNotNull, filterPartials, mergeAndCache, shareReplayRefCount, startFrom, switchMapCatchError, tapAny, tapPageInfoHeaders } from "../../../shared/rxjs-operators";

export declare type HasId = { id: string };
export declare type PostChange = { mode: "new" | "edited" | "deleted", post: HasId };


@Component({
  templateUrl: "./list-latest.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ListLatestComponent {
  public authService: AuthService = inject(AuthService);
  private postService: PostService = inject(PostService);
  private formBuilder: FormBuilder = inject(FormBuilder)
  private postsHubService: PostsHub = inject(PostsHub);

  listFilters = this.formBuilder.group<ListFilters>({
    count: 10,
    hideModActions: false,
    hasReplay: undefined,
  });

  loaded$ = new BehaviorSubject<boolean>(false);

  pageRequest$ = new BehaviorSubject(1);

  pageInfo = new BehaviorSubject<{ currentPage: number, pageSize: number, totalItems: number, totalPages: number } | null>({ currentPage: 1, pageSize: 10, totalItems: 0, totalPages: 0 });
  pageInfo$ = this.pageInfo.asObservable().pipe(
    tap(pageInfo => console.debug("pageInfo", pageInfo)),
    filterNotNull(),
    distinctUntilChanged(),
    shareReplayRefCount(1),
  );


 _posts$ = combineLatest([
    this.listFilters.valueChanges.pipe(startWith(this.listFilters.value)),
    this.pageRequest$
  ]).pipe(
    //debounceTime(300),
    distinctUntilChanged(),
    filterPartials(),
    filterNotNull(),
    tap(() => this.loaded$.next(false)),
    switchMapCatchError(([filters, page]) => this.postService.apiPostLatestGet$Json$Response({
      pageSize: filters.count!,
      page: page,
      hasReplay: filters.hasReplay!,
      hideModActions: filters.hideModActions!,
    })),
    tapPageInfoHeaders(this.pageInfo),
    map(r => r!.body),
    shareReplayRefCount(1)
  );

  private postChanges$ = combineLatest([
    this.postsHubService.newPost$.pipe(startWith(null)),
    this.postsHubService.editedPost$.pipe(startWith(null)),
    this.postsHubService.deletedPost$.pipe(startWith(null)),
  ]).pipe(
    map(([newPost, editedPost, deletedPost]) => {
      if (newPost)
        return {mode: "new", post: newPost} as PostChange;
      else if (editedPost)
        return {mode: "edited", post: editedPost} as PostChange;
      else
        return {mode: "deleted", post: {id: deletedPost}} as PostChange;
    }));

  posts$ = this._posts$.pipe(
    filterNotNull(),
    mergeAndCache(this.postChanges$, this.merge)
  );

  private merge(array: PlayerPostDto[], postChange: PostChange) {
    if (postChange.mode == "new" && !array.find(a => a.id == postChange.post.id)) {
      array = [postChange.post, ...array];
    } else if (postChange.mode == "edited") {
      array[array.findIndex(a => a.id == postChange.post.id)] = postChange.post;
      array = [...array];
    } else {
      array = [...array.splice(array.findIndex(a => a.id == postChange.post.id), 1)];
    }
    return array;
  }

  public onPageChanged(page: number) {
    this.pageRequest$.next(page);
    console.debug("page changed", page);
  }
}

export type ListFilters = {
  count?: number,
  hasReplay?: boolean,
  hideModActions: boolean
};
