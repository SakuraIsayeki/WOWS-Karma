import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { ChatMessageChannelPipe } from "src/app/services/pipes/chat-message-channel.pipe";

@Component({
  selector: 'replay-chat-log',
  templateUrl: './chat-log.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [
    CommonModule,
    RouterLink,
    ChatMessageChannelPipe
  ]
})
export class ChatLogComponent {
  public replay = input.required<ReplayDto>();
  public authorId = input.required<number>();
  public playerId = input.required<number>();

  public whoChatted = computed(() => [
    this.replay().chatMessages?.some(msg => msg.playerId === this.authorId()) ?? false,
    this.replay().chatMessages?.some(msg => msg.playerId === this.playerId()) ?? false
  ]);
}
