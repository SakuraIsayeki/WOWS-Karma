import { Pipe, PipeTransform } from '@angular/core';
import { getWowsNumbersClanLink } from "../helpers";

@Pipe({
  standalone: true,
  name: 'wowsNumbersClanLink'
})
export class WowsNumbersClanLinkPipe implements PipeTransform {

  transform(clan: { id?: number, tag?: string | null, name?: string | null}): unknown {
    return getWowsNumbersClanLink({ id: clan.id!, tag: clan.tag!, name: clan.name! });
  }

}
