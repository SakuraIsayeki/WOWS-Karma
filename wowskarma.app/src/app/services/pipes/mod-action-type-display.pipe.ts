import { Pipe, PipeTransform } from "@angular/core";
import { ModActionType } from 'src/app/services/api/models/mod-action-type';
import { PostFlairs } from "../api/models/post-flairs";
import { getKarmaColor, getPostBorderColor } from "../helpers";

@Pipe({
  standalone: true,
  name: "modActionTypeDisplay",
})
export class ModActionTypeDisplayPipe implements PipeTransform {
  transform(value: ModActionType) {
    switch (value) {
      case ModActionType.Update:
        return {text: "Edit", color: "text-warning"};
      case ModActionType.Delete:
        return {text: "Deletion", color: "text-danger"};
    }
  }
}
