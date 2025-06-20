import { ChangeDetectionStrategy, Component, input, Input } from "@angular/core";
import { NgbTooltip } from "@ng-bootstrap/ng-bootstrap";
import { KarmaColorPipe } from "../../../services/pipes/karma-color.pipe";

@Component({
    selector: "minmax-metric",
    templateUrl: "./min-max-metric.component.html",
    styleUrls: ["./min-max-metric.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [NgbTooltip, KarmaColorPipe],
})
export class MinMaxMetricComponent {
    metric = input<MinMaxMetricObject>();
    name = input.required<string>();

    constructor() { }
}

export type MinMaxMetricObject = { total: any; min: number; max: number };