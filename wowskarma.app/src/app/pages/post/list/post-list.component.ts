import { ChangeDetectionStrategy, Component } from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { BehaviorSubject, combineLatest, debounceTime, distinctUntilChanged, startWith, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerPostDto } from "../../../services/api/models/player-post-dto";
import { PostService } from "../../../services/api/services/post.service";
import { AuthService } from "../../../services/auth.service";
import { PostsHubService } from "../../../services/hubs/posts-hub.service";
import {
  filterNotNull,
  filterPartials, mergeAndCache,
  shareReplayRefCount,
  startFrom,
  switchMapCatchError,
  tapAny,
} from "../../../shared/rxjs-operators";

export declare type HasId = {id: string};
export declare type PostChange = { mode: 'new' | 'edited' | 'deleted', post: HasId };



@Component({
  templateUrl: "./post-list.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostListComponent {
  listFilters = this.formBuilder.group<ListFilters>({
    count: 10,
    hideModActions: false,
    hasReplay: undefined
  });

  loaded$ = new BehaviorSubject<boolean>(false);

  _posts$ = this.listFilters.valueChanges.pipe(
    debounceTime(300),
    distinctUntilChanged(),
    filterPartials(),
    filterNotNull(),
    tap(() => this.loaded$.next(false)),
    switchMapCatchError(filters => this.postService.apiPostLatestGet$Json({
      count: filters.count!,
      hasReplay: filters.hasReplay!,
      hideModActions: filters.hideModActions!,
    }).pipe(
      tapAny(() => this.loaded$.next(true)),
      tap(posts => console.debug("DEBUG: posts", posts)),
    )),
    startFrom(this.postService.apiPostLatestGet$Json({ count: 10 })),
    shareReplayRefCount(1),
  );



  private postChanges$ = combineLatest([
      this.postsHubService.newPost$.pipe(startWith(null)),
    this.postsHubService.editedPost$.pipe(startWith(null)),
    this.postsHubService.deletedPost$.pipe(startWith(null))
  ])
      .pipe(map(([newPost, editedPost, deletedPost]) => {
        if(newPost)
          return { mode: 'new', post: newPost } as PostChange;
        else if(editedPost)
          return { mode: 'edited', post: editedPost } as PostChange;
        else
          return { mode: 'deleted', post: {id: deletedPost}} as PostChange;
      }))
  posts$ = this._posts$.pipe(filterNotNull(),
      mergeAndCache(this.postChanges$, this.merge));

  constructor(public authService: AuthService, private postService: PostService, private formBuilder: FormBuilder, private postsHubService: PostsHubService) {

  }

  private merge(array: PlayerPostDto[], postChange: PostChange){
    if(postChange.mode == 'new' && !array.find(a => a.id == postChange.post.id)){
      array = [postChange.post, ...array];
    } else if(postChange.mode == 'edited'){
      array[array.findIndex(a => a.id == postChange.post.id)] = postChange.post;
      array = [...array];
    } else{
      array = [...array.splice(array.findIndex(a => a.id == postChange.post.id), 1)];
    }
    return array;
  }
}

export type ListFilters = {
  count?: number,
  hasReplay?: boolean,
  hideModActions: boolean
};
