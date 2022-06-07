/* tslint:disable */
/* eslint-disable */
import { ModActionType } from './mod-action-type';
import { PlayerPostDto } from './player-post-dto';
export interface PostModActionDto {
  actionType?: ModActionType;
  id?: string;
  modId?: number;
  modUsername?: null | string;
  postId: string;
  reason: string;
  updatedPost?: PlayerPostDto;
}
