import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { AccountClanListingDto } from 'src/app/services/api/models/account-clan-listing-dto';
import { ClanListingDto } from 'src/app/services/api/models/clan-listing-dto';
import { NgIf } from "@angular/common";
import { ColorHexPipe } from "../../../services/pipes/colorHex.pipe";
import { RouterLink } from "@angular/router";

@Component({
  selector: 'player-namelink',
  templateUrl: './player-namelink.component.html',
  imports: [
    NgIf,
    ColorHexPipe,
    RouterLink
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class PlayerNamelinkComponent {
  player = input<{ id?: number, username?: string, clan?: { id?: number, leagueColor?: number, name?: string, tag?: string; } }>();
  displayClan = input(false);

  constructor() {
  }
}
