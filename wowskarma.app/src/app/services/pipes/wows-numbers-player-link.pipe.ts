import { Pipe, PipeTransform } from "@angular/core";
import { getWowsNumbersPlayerLink } from "../helpers";

@Pipe({
    name: "wowsNumbersPlayerLink",
    standalone: true,
})
export class WowsNumbersPlayerLinkPipe implements PipeTransform {

    transform(player: { id?: number, username?: string | null }) {
        return getWowsNumbersPlayerLink({ id: player.id!, username: player.username! });
    }

}
