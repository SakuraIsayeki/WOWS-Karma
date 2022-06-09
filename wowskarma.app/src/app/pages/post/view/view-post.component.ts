import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { ActivatedRoute } from "@angular/router";
import { PostService } from "../../../services/api/services/post.service";
import { routeParam, shareReplayRefCount, switchMapCatchError } from "../../../shared/rxjs-operators";

@Component({
  templateUrl: './view-post.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ViewPostComponent {
// Get the "ID,username" from the route params.
  post$ = routeParam(this.route, "postId")
    .pipe(switchMapCatchError((postId) =>
        this.postService.apiPostPostIdGet$Json({ postId: postId as string })),
      shareReplayRefCount(1));

  constructor(private route: ActivatedRoute, private postService: PostService) {
  }
}
