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
  readOnly?: boolean;
  player?: AccountClanListingDto;
  replay?: ReplayDto;
  replayId?: null | string;
  replayState?: ReplayState
  title?: null | string;
  updatedAt?: null | string;
  supportTicketStatus?: {
    hasTicket: boolean;
    ticketId: number | null;
  }
}

enum ReplayState {
  NoReplay,
  Processing,
  Ready
}
