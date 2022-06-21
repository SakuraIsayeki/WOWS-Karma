import { Pipe, PipeTransform } from "@angular/core";
import { PostFlairs } from "../api/models/post-flairs";
import { getKarmaColor, getPostBorderColor } from "../helpers";

@Pipe({
    name: "karmaColor",
})
export class KarmaColorPipe implements PipeTransform {
    transform(value: number | { flairs: PostFlairs | undefined } | null | undefined): "success" | "danger" | "warning" {
        if (typeof value === "number") {
            return getKarmaColor(value);
        }

        if (value && value.flairs) {
            return getPostBorderColor(value);
        }

        return "warning";
    }
}
