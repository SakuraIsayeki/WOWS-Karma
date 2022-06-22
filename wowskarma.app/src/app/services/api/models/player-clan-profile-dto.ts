/* tslint:disable */
/* eslint-disable */
import { ClanListingDto } from './clan-listing-dto';
import { ClanRole } from './clan-role';
export interface PlayerClanProfileDto {
  clanInfo?: ClanListingDto;
  clanMemberRole?: ClanRole;
  joinedClanAt?: string;
}
