import { ChangeDetectionStrategy, Component, Input } from "@angular/core";
import { Observable } from "rxjs";
import { map } from "rxjs/operators";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { InputObservable } from "../../rxjs-operators";

@Component({
  selector: 'replay-chat-log',
  templateUrl: './chat-log.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChatLogComponent {
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

  messages = this.replay.chatMessages;
  public messages$ = this.replay$.pipe(
    map(replay => replay.chatMessages),
  );

  public authorChatted$ = this.messages$.pipe(
    map(messages => messages?.some(message => message.playerId === this.authorId) ?? false)
  );

  public playerChatted$ = this.messages$.pipe(
    map(messages => messages?.some(message => message.playerId === this.playerId) ?? false)
  );

  //getChannelName(msgCategory: ChatMessageCategory): string {
}
