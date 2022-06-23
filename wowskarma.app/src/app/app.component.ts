import { Component } from '@angular/core';
import { AppInitService } from "./services/app-init.service";

@Component({
  template: "<router-outlet></router-outlet>",

})
export class AppComponent {
  constructor(private appInitService: AppInitService) {
  }

  ngOnInit(): void {
      setTimeout(() => this.appInitService.initialized(), 0);
  }
}
