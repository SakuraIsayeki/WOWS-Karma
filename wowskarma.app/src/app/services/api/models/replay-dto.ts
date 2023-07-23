/* tslint:disable */
/* eslint-disable */
import { ReplayChatMessageDto } from './replay-chat-message-dto';
import { ReplayPlayerDto } from './replay-player-dto';
export interface ReplayDto {
  chatMessages?: null | Array<ReplayChatMessageDto>;
  downloadUri?: null | string;
  minimapUri?: null | string;
  id?: string;
  players?: null | Array<ReplayPlayerDto>;
  postId?: string;
}
