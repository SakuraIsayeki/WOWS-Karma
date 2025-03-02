import { ChangeDetectionStrategy, Component, computed, input, Input } from "@angular/core";
import { combineLatest, Observable } from "rxjs";
import { map } from "rxjs/operators";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { ReplayPlayerDto } from "../../../services/api/models/replay-player-dto";
import { InputObservable, shareReplayRefCount } from "../../rxjs-operators";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";

@Component({
  selector: "replay-team-roster",
  templateUrl: "./team-roster.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterLink
  ]
})
export class TeamRosterComponent {
  replay = input.required<ReplayDto>()
  authorId = input.required<number>()
  playerId = input.required<number>()

  public author = computed(() => this.replay().players!.find((player) => player.accountId === this.authorId()));
  public player = computed(() => this.replay().players!.find((player) => player.accountId === this.playerId()));

  // Map teams based on replay.players teamIds
  public teams = computed(() => {
    const teams: ReplayPlayerDto[][] = [];

    for (const player of this.replay().players!) {
      if (!teams[player.teamId!]) {
        teams[player.teamId!] = [];
      }

      teams[player.teamId!].push(player);
    }

    return teams;
  });


  getMaxPlayersInOneTeam(teams: ReplayPlayerDto[][]) {
    return Math.max(...teams
      .filter(Boolean)
      .map((team) => team?.length || 0));
  }

  createRange(number: number) {
    return new Array(number);
  }

  filterNotNullOrEmpty(value: any[]) {
    return value && value.length > 0;
  }
}
