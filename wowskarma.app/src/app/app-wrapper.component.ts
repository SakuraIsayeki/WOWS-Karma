import { ChangeDetectionStrategy, Component, OnInit } from '@angular/core';
import { AppInitService } from "./services/app-init.service";

@Component({
  selector: 'app-root',
  templateUrl: './app-wrapper.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class AppWrapperComponent implements OnInit {

  constructor(public appInit: AppInitService){

  }

  ngOnInit(): void {
  }

}
