import { ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { AccountClanListingDto } from 'src/app/services/api/models/account-clan-listing-dto';
import { ClanListingDto } from 'src/app/services/api/models/clan-listing-dto';

@Component({
  selector: 'player-namelink',
  templateUrl: './player-namelink.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlayerNamelinkComponent {
  @Input() player?: { id?: number, username?: string, clan?: { id?: number, leagueColor?: number, name?: string, tag?: string; } } | undefined;
  @Input() displayClan = false;

  constructor() {
  }
}
