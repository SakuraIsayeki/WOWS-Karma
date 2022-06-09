import { ChangeDetectionStrategy, Component, Input, OnInit } from "@angular/core";
import { PlayerPostDto } from "../../services/api/models/player-post-dto";
import { getPostBorderColor } from "../../services/helpers";

@Component({
  selector: "app-post",
  templateUrl: "./post.component.html",
  styleUrls: ["./post.component.scss"],
  changeDetection: ChangeDetectionStrategy.OnPush,
  inputs: ["post", "postDisplayType"],

})

//TODO: Implement editor modals

export class PostComponent {
  @Input() public post?: PlayerPostDto;
  @Input() public postDisplayType?: "neutral" | "received" | "sent";

  getPostBorderColor({ flairs }: PlayerPostDto) {
    return getPostBorderColor({ flairs });
  }
}
