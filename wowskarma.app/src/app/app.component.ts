import { Component, inject, type OnInit } from "@angular/core";
import { ActivatedRouteSnapshot, ResolveEnd, Router, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs/operators';
import { AppInitService } from "./services/app-init.service";
import { AppInsightsService } from "./services/app-insights.service";

@Component({
    template: "<router-outlet></router-outlet>",
    standalone: true,
    imports: [RouterOutlet],
})
export class AppComponent implements OnInit {
  private router = inject(Router);
  private appInitService = inject(AppInitService);
  private appInsightsService = inject(AppInsightsService);

  ngOnInit() {
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
