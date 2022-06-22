/* tslint:disable */
/* eslint-disable */
import { PlayerClanProfileDto } from './player-clan-profile-dto';
export interface PlayerProfileDto {
  clan?: PlayerClanProfileDto;
  gameKarma?: number;
  id?: number;
  lastBattleTime?: string;
  negativeKarmaAble?: boolean;
  optOutChanged?: string;
  optedOut?: boolean;
  postsBanned?: boolean;
  ratingCourtesy?: number;
  ratingPerformance?: number;
  ratingTeamplay?: number;
  siteKarma?: number;
  username?: null | string;
  wgAccountCreatedAt?: string;
  wgHidden?: boolean;
}
