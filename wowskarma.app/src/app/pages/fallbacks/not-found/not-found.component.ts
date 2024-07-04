import { ChangeDetectionStrategy, Component, input } from '@angular/core';
import { NgOptimizedImage } from "@angular/common";

@Component({
  standalone: true,
  selector: 'app-not-found',
  templateUrl: './not-found.component.html',
  styleUrls: ['./not-found.component.scss'],
  imports: [
    NgOptimizedImage
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class NotFoundComponent {
  message = input("Sorry, there's nothing at this address.");
}
