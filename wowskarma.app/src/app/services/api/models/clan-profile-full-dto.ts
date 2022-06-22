/* tslint:disable */
/* eslint-disable */
import { PlayerProfileDto } from './player-profile-dto';
export interface ClanProfileFullDto {
  createdAt?: string;
  description?: null | string;
  id?: number;
  isDisbanded?: boolean;
  leagueColor?: number;
  members?: null | Array<PlayerProfileDto>;
  membersUpdatedAt?: string;
  name?: null | string;
  tag?: null | string;
  updatedAt?: string;
}
