import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ColorHexPipe } from 'src/app/services/pipes/colorHex.pipe';

@Component({
  selector: 'player-namelink',
  templateUrl: './player-namelink.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    RouterLink,
    ColorHexPipe
  ]
})
export class PlayerNamelinkComponent {
  player = input<{ id?: number, username?: string, clan?: { id?: number, leagueColor?: number, name?: string, tag?: string; } }>();
  displayClan = input(false);

  constructor() {
  }
}
