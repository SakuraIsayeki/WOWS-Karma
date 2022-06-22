import { ChangeDetectionStrategy, Component, OnInit } from "@angular/core";
import { FormBuilder } from "@angular/forms";
import { BehaviorSubject, debounceTime, distinctUntilChanged, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PostService } from "../../../services/api/services/post.service";
import { AuthService } from "../../../services/auth.service";
import { sortByCreationDate } from "../../../services/helpers";
import {
  filterNotNull,
  filterPartials,
  shareReplayRefCount,
  startFrom,
  switchMapCatchError,
  tapAny,
} from "../../../shared/rxjs-operators";

@Component({
  templateUrl: "./post-list.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class PostListComponent {
  listFilters = this.formBuilder.group({
    count: 10,
    hideModActions: false,
    hasReplay: undefined
  });

  loaded$ = new BehaviorSubject<boolean>(false);

  posts$ = this.listFilters.valueChanges.pipe(
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

  constructor(public authService: AuthService, private postService: PostService, private formBuilder: FormBuilder) {
  }
}

export type ListFilters = {
  count?: number,
  hasReplay?: boolean,
  hideModActions: boolean
};
