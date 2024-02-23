import { Injectable } from '@angular/core';
import { ApplicationInsights } from "@microsoft/applicationinsights-web";
import { AuthModel } from 'src/app/models/AuthModel';
import { AppConfigService } from 'src/app/services/app-config.service';
import { AuthService } from 'src/app/services/auth.service';
import { environment } from "../../environments/environment";

@Injectable()
export class AppInsightsService {
  appInsights: ApplicationInsights;

  constructor(private authService: AuthService) {
    this.appInsights = new ApplicationInsights({
      config: {
        connectionString: environment.applicationInsights.connectionString,
        enableAutoRouteTracking: true // option to log all route changes
      }
    });

    this.appInsights.loadAppInsights();

    this.authService.userInfo$.subscribe(auth => {
      if (auth) {
        this.appInsights.setAuthenticatedUserContext(auth.id.toString(), auth.id.toString(), true);
      }
      else {
        this.appInsights.clearAuthenticatedUserContext();
      }
    });

    this.appInsights.addTelemetryInitializer(item => {
      item.tags ||= []

      item.tags["ai.cloud.role"] = "wowskarma.app";
      item.tags["ai.cloud.roleInstance"] = "wowskarma.app";
      item.tags["region"] = AppConfigService.GetApiRegionFromLocation()?.toString();
    });
  }

  logPageView(name?: string, url?: string) { // option to call manually
    this.appInsights.trackPageView({
      name: name,
      uri: url
    });
  }

  logEvent(name: string, properties?: { [key: string]: any }) {
    this.appInsights.trackEvent({name: name}, properties);
  }

  logMetric(name: string, average: number, properties?: { [key: string]: any }) {
    this.appInsights.trackMetric({name: name, average: average}, properties);
  }

  logException(exception: Error, severityLevel?: number) {
    this.appInsights.trackException({exception: exception, severityLevel: severityLevel});
  }

  logTrace(message: string, properties?: { [key: string]: any }) {
    this.appInsights.trackTrace({message: message}, properties);
  }
}
