import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { ReplayDto } from "../../../services/api/models/replay-dto";
import { CommonModule } from "@angular/common";
import { RouterLink } from "@angular/router";
import { ChatMessageChannelPipe } from "src/app/services/pipes/chat-message-channel.pipe";

@Component({
  selector: 'replay-chat-log',
  templateUrl: './chat-log.component.html',
  styleUrls: ['./chat-log.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush,
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    ChatMessageChannelPipe
  ]
})
export class ChatLogComponent {
  public readonly replay = input.required<ReplayDto>();
  public readonly authorId = input.required<number>();
  public readonly playerId = input.required<number>();

  public readonly whoChatted = computed(() => [
    this.replay().chatMessages?.some(msg => msg.playerId === this.authorId()) ?? false,
    this.replay().chatMessages?.some(msg => msg.playerId === this.playerId()) ?? false
  ]);
}
