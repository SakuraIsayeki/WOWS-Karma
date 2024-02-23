import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AccountClanListingDto } from 'src/app/services/api/models/account-clan-listing-dto';
import { ClanListingDto } from 'src/app/services/api/models/clan-listing-dto';

@Component({
  selector: 'player-namelink',
  templateUrl: './player-namelink.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlayerNamelinkComponent {
  player = input<{ id?: number, username?: string, clan?: { id?: number, leagueColor?: number, name?: string, tag?: string; } }>();
  displayClan = input(false);

  constructor() {
  }
}
