import { ChangeDetectionStrategy, Component, Input, OnInit } from "@angular/core";
import { PostFlairs } from "../../../services/api/models/post-flairs";
import { parseFlairsEnum } from "../../../services/metricsHelpers";
import { NgFor, NgIf } from "@angular/common";

@Component({
    selector: "flairs-markup",
    templateUrl: "./flair-markups.component.html",
    changeDetection: ChangeDetectionStrategy.OnPush,
    inputs: ["flairsEnum"],
    standalone: true,
    imports: [NgFor, NgIf],
})
export class FlairMarkupsComponent implements OnInit {
  // Post Flairs
  @Input() flairsEnum?: PostFlairs;
  flairNames = ["Performance", "Teamplay", "Courtesy"];

  constructor() {
  }

  ngOnInit(): void {
  }

  parseFlairsEnum(flairs: PostFlairs | null) {
    return parseFlairsEnum(flairs);
  }
}
