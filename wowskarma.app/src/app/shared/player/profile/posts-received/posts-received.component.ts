import { ChangeDetectionStrategy, Component, Input, OnInit } from "@angular/core";
import { BehaviorSubject, Observable, tap } from "rxjs";
import { PlayerPostDto } from "../../../../services/api/models/player-post-dto";
import { PostService } from "../../../../services/api/services/post.service";
import { sortByCreationDate } from "../../../../services/helpers";
import { InputObservable, switchMapCatchError, tapAny } from "../../../rxjs-operators";

@Component({
  selector: 'app-posts-received',
  templateUrl: './posts-received.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})

export class PostsReceivedComponent {
  @Input()
  @InputObservable()
  userId!: number;
  userId$!: Observable<number>;

  loaded$ = new BehaviorSubject(false);

  // Get an observable to fetch received posts on component init.
  receivedPosts$ = this.userId$.pipe(
    tap(() => this.loaded$.next(false)),
    switchMapCatchError(userId => this.postService.apiPostUserIdReceivedGet$Json({ userId })),
    tapAny(() => this.loaded$.next(true))
  );

  constructor(private postService: PostService) { }

  sortByLastCreated(a: PlayerPostDto, b: PlayerPostDto) {
    return sortByCreationDate(a, b, true);
  }
}
