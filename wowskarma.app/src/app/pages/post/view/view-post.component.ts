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
  post$ = routeParam(this.route)
    .pipe(switchMapCatchError((id) =>
        this.postService.apiPostPostIdGet$Json({ postId: id! })),
      shareReplayRefCount(1));

  constructor(private route: ActivatedRoute, private postService: PostService) {
  }
}
