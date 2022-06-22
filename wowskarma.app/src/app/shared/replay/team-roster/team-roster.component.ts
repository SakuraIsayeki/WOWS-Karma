import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { combineLatest, Observable } from "rxjs";
import { map } from "rxjs/operators";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { ReplayPlayerDto } from "../../../services/api/models/replay-player-dto";
import { InputObservable, shareReplayRefCount } from "../../rxjs-operators";

@Component({
  selector: "replay-team-roster",
  templateUrl: "./team-roster.component.html",
  changeDetection: ChangeDetectionStrategy.OnPush,
  inputs: ["replay", "authorId", "playerId"],
})
export class TeamRosterComponent {
  @Input()
  @InputObservable()
  public replay!: ReplayDto;
  public replay$!: Observable<ReplayDto>;

  @Input()
  @InputObservable()
  public authorId!: number;
  public authorId$!: Observable<number>;

  @Input()
  @InputObservable()
  public playerId!: number;
  public playerId$!: Observable<number>;


  public author$ = combineLatest([this.replay$, this.authorId$]).pipe(
    map(([replay, authorId]) => {
      return replay.players!.find((player) => player.accountId === authorId)!;
    }),
    shareReplayRefCount(1));

  // Match playerId$ from player.Id
  public player$ = combineLatest([this.replay$, this.playerId$]).pipe(
    map(([replay, playerId]) => {
      return replay.players!.find((player) => player.accountId === playerId)!;
    }),
    shareReplayRefCount(1));

  // Map teams based on replay.players teamIds
  public teams$: Observable<ReplayPlayerDto[][]> = this.replay$.pipe(
    map((replay) => {
      const teams: ReplayPlayerDto[][] = [];

      for (const player of replay.players!) {
        if (teams[player.teamId!] === undefined) {
          teams[player.teamId!] = [];
        }

        teams[player.teamId!].push(player);
      }

      return teams;
    }));


  getMaxPlayersInOneTeam(teams: ReplayPlayerDto[][]) {
    return Math.max(...teams.map((team) => team.length));
  }

  createRange(number: number) {
    return new Array(number);
  }
}
