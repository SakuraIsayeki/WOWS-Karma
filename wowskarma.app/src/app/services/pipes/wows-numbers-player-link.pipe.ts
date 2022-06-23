import { Pipe, PipeTransform } from "@angular/core";
import { getWowsNumbersPlayerLink } from "../helpers";

@Pipe({
    name: "wowsNumbersPlayerLink",
})
export class WowsNumbersPlayerLinkPipe implements PipeTransform {

    transform(player: { id?: number, username?: string }) {
        return getWowsNumbersPlayerLink({ id: player.id!, username: player.username! });
    }

}
