import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { BehaviorSubject, Observable, tap } from "rxjs";
import { map } from "rxjs/operators";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { InputObservable, switchMapCatchError, tapAny } from "../../../rxjs-operators";

@Component({
  selector: 'app-posts-sent',
  templateUrl: './posts-sent.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PostsSentComponent {
  @Input()
  @InputObservable()
  userId!: number;
  userId$!: Observable<number>;

  loading$ = new BehaviorSubject(false);

  // Get an observable to fetch received posts on component init.
  sentPosts$ = this.userId$.pipe(
    tap(() => this.loading$.next(true)),
    switchMapCatchError(userId => this.postService.apiPostUserIdSentGet$Json({ userId })),
    map(posts => posts?.sort(this.sortByLastCreated)),
    tapAny(() => this.loading$.next(false))
  );

  constructor(private postService: PostService) { }

  sortByLastCreated(a: PlayerPostDto, b: PlayerPostDto) {
    return sortByCreationDate(a, b, true);
  }
}
