import { ChangeDetectionStrategy, Component, OnInit, inject } from '@angular/core';
import { AppInitService } from "./services/app-init.service";
import { NgIf, AsyncPipe } from '@angular/common';
import { RouterOutlet } from '@angular/router';

@Component({
    selector: 'app-root',
    templateUrl: './app-wrapper.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    standalone: true,
    imports: [NgIf, RouterOutlet, AsyncPipe]
})
export class AppWrapperComponent {
  appInit = inject(AppInitService);
}
