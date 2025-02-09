import { Component } from "@angular/core";
import { ActivatedRouteSnapshot, ResolveEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AppInitService } from "./services/app-init.service";
import { AppInsightsService } from "./services/app-insights.service";

@Component({
  template: "<router-outlet />",

  imports: [
    RouterOutlet
  ]
})
export class AppComponent {
  constructor(
    private router: Router,
    private appInitService: AppInitService,
    private appInsightsService: AppInsightsService, // Used for initializing the app insights service
  ) {
  }

  ngOnInit(): void {
    setTimeout(() => this.appInitService.initialized(), 0);

    this.router.events
      .pipe(filter((event): event is ResolveEnd => event instanceof ResolveEnd))
      .subscribe((event) => {
        const activatedComponent = this.getActivatedComponent(event.state.root);
        if (activatedComponent) {
          this.appInsightsService.logPageView(activatedComponent.id, event.url);
        }
      });
  }

  private getActivatedComponent(snapshot: ActivatedRouteSnapshot): any {
    if (snapshot.firstChild) {
      return this.getActivatedComponent(snapshot.firstChild);
    }

    return snapshot.component;
  }
}
