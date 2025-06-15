import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { AppInitService } from "./services/app-init.service";
import { RouterOutlet } from "@angular/router";
import { AsyncPipe, NgIf } from "@angular/common";

@Component({
  selector: 'app-root',
  templateUrl: './app-wrapper.component.html',
  imports: [
    RouterOutlet,
    NgIf,
    AsyncPipe
  ],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppWrapperComponent {

  constructor(public appInit: AppInitService) {
  }
}
