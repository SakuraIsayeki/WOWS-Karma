import { ChangeDetectionStrategy, Component, Input } from "@angular/core";

@Component({
    selector: "minmax-metric",
    templateUrl: "./min-max-metric.component.html",
    styleUrls: ["./min-max-metric.component.scss"],
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class MinMaxMetricComponent {
    @Input() metric?: MinMaxMetricObject | null;
    @Input() name!: string;

    constructor() { }
}

export type MinMaxMetricObject = { total: any; min: number; max: number };