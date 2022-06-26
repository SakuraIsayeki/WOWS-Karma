import { Component } from "@angular/core";
import { AppInitService } from "./services/app-init.service";
import { AppInsightsService } from "./services/app-insights.service";

@Component({
    template: "<router-outlet></router-outlet>",

})
export class AppComponent {
    constructor(
        private appInitService: AppInitService,
        private _applicationInsights: AppInsightsService, // Used for initializing the app insights service
    ) {
    }

    ngOnInit(): void {
        setTimeout(() => this.appInitService.initialized(), 0);
    }
}
