import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AccountClanListingDto } from 'src/app/services/api/models/account-clan-listing-dto';
import { ClanListingDto } from 'src/app/services/api/models/clan-listing-dto';
import { NgIf } from '@angular/common';
import { RouterLink } from '@angular/router';
import { ColorHexPipe } from '../../../services/pipes/colorHex.pipe';

@Component({
    selector: 'player-namelink',
    templateUrl: './player-namelink.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [NgIf, RouterLink, ColorHexPipe]
})
export class PlayerNamelinkComponent {
  player = input<{ id?: number, username?: string, clan?: { id?: number, leagueColor?: number, name?: string, tag?: string; } }>();
  displayClan = input(false);

  constructor() {
  }
}
