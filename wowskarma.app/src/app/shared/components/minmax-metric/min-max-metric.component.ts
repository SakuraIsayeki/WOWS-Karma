import { ChangeDetectionStrategy, Component, input, Input } from "@angular/core";
import { KarmaColorPipe } from "src/app/services/pipes/karma-color.pipe";

@Component({
    selector: "minmax-metric",
    templateUrl: "./min-max-metric.component.html",
    styleUrls: ["./min-max-metric.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        KarmaColorPipe
    ]
})
export class MinMaxMetricComponent {
    metric = input<MinMaxMetricObject>();
    name = input.required<string>();

    constructor() { }
}

export type MinMaxMetricObject = { total: any; min: number; max: number };