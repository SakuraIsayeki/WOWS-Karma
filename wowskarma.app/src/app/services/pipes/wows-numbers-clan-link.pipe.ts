import { Pipe, type PipeTransform } from '@angular/core';
import { getWowsNumbersClanLink } from "../helpers";

@Pipe({
    name: 'wowsNumbersClanLink',
    standalone: true
})
export class WowsNumbersClanLinkPipe implements PipeTransform {

  transform(clan: { id?: number, tag?: string | null, name?: string | null}) {
    return getWowsNumbersClanLink({ id: clan.id!, tag: clan.tag!, name: clan.name! });
  }

}
