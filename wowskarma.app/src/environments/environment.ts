// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
    name: "development",

    apiHost: {
        EU: "http://localhost:5010",
        NA: "http://localhost:5010",
        CIS: "http://localhost:5010",
        SEA: "http://localhost:5010",
    },

    applicationInsights: {
        connectionString: "InstrumentationKey=3fab5b95-b531-4f15-95e9-83f31e2ed531;IngestionEndpoint=https://francecentral-1.in.applicationinsights.azure.com/;LiveEndpoint=https://francecentral.livediagnostics.monitor.azure.com/",
    },

    cookies: {
        domain: "localhost",

        name: {
            EU: "Auth_EU",
            NA: "Auth_NA",
            CIS: "Auth_CIS",
            SEA: "Auth_SEA",
        },
    },
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
import "zone.js/plugins/zone-error"; // Included with Angular CLI.
