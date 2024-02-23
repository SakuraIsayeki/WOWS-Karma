import { ChangeDetectionStrategy, Component, computed, input, Input } from "@angular/core";
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
  public replay = input.required<ReplayDto>();
  public authorId = input.required<number>();
  public playerId = input.required<number>();

  public whoChatted = computed(() => [
    this.replay().chatMessages?.some(msg => msg.playerId === this.authorId()) ?? false,
    this.replay().chatMessages?.some(msg => msg.playerId === this.playerId()) ?? false
  ]);
}
