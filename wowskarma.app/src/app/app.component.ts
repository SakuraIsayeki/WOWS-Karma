import { Component } from '@angular/core';
import { AppInitService } from "./services/app-init.service";

@Component({
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],

})
export class AppComponent {
  title = 'wowskarma.app';

  constructor(private appInitService: AppInitService) {
  }


  ngOnInit(): void {
      setTimeout(() => this.appInitService.initialized(), 0);
  }
}
