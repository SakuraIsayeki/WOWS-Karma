/* tslint:disable */
/* eslint-disable */
import { AccountClanListingDto } from './account-clan-listing-dto';
import { PostFlairs } from './post-flairs';
import { ReplayDto } from './replay-dto';
export interface PlayerPostDto {
  author?: AccountClanListingDto;
  content?: null | string;
  createdAt?: null | string;
  flairs?: PostFlairs;
  id?: null | string;
  modLocked?: boolean;
  player?: AccountClanListingDto;
  replay?: ReplayDto;
  replayId?: null | string;
  title?: null | string;
  updatedAt?: null | string;
}
