import { Pipe, PipeTransform } from '@angular/core';
import { ReplayChatMessageDto } from "../api/models/replay-chat-message-dto";

@Pipe({
  name: 'chatMessageChannel'
})
export class ChatMessageChannelPipe implements PipeTransform {

  transform(value: string | null | undefined) {
    switch (value) {
      case "battle_common":
        return { display: "All", color: "" };
      case "battle_team":
        return { display: "Team", color: "text-success" };
      case "battle_prebattle":
        return { display: "Division", color: "text-warning" };
      default:
        return { display: "Unknown", color: "text-muted" };
    }
  }
}
