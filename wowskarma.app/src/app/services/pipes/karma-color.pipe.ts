import { Pipe, PipeTransform } from "@angular/core";
import { PostFlairs } from "../api/models/post-flairs";
import { getKarmaColor, getPostBorderColor } from "../helpers";

@Pipe({
    name: "karmaColor",
})
export class KarmaColorPipe implements PipeTransform {
    transform(value: number | undefined | null): "success" | "danger" | "warning" {
        if (typeof value === "number") {
            return getKarmaColor(value);
        }

        return "warning";
    }
}
