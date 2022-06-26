import { ChangeDetectionStrategy, Component } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { filter, map } from "rxjs/operators";
import { ModActionService } from "../../../services/api/services/mod-action.service";
import { PostService } from "../../../services/api/services/post.service";
import { filterNotNull, routeParam, shareReplayRefCount, switchMapCatchError } from "../../../shared/rxjs-operators";

@Component({
    templateUrl: "./view-post.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ViewPostComponent {
// Get the "ID,username" from the route params.
    post$ = routeParam(this.route)
        .pipe(switchMapCatchError((id) =>
                this.postService.apiPostPostIdGet$Json({ postId: id! })),
            shareReplayRefCount(1));

    lastModAction$ = this.post$.pipe(
        filterNotNull(),
        filter((post) => post.modLocked === true),
        switchMapCatchError((post) => this.modActionService.apiModActionListGet$Json({ postId: post.id! }).pipe(
            filterNotNull(),
        )),
        map((modActions) => modActions![0]),
        shareReplayRefCount(1),
    );

    constructor(private route: ActivatedRoute, private postService: PostService, private modActionService: ModActionService) {
    }
}
