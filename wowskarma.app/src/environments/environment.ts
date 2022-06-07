// This file can be replaced during build by using the `fileReplacements` array.
// `ng build` replaces `environment.ts` with `environment.prod.ts`.
// The list of file replacements can be found in `angular.json`.

export const environment = {
  production: false,

  apiHost: {
    EU: "http://localhost:5010/api/",
    NA: "http://localhost:5010/api/",
    CIS: "http://localhost:5010/api/",
    SEA: "http://localhost:5010/api/",
  },

  cookies: {
    domain: "localhost",

    name: {
      EU: "Auth_EU",
      NA: "Auth_NA",
      CIS: "Auth_CIS",
      SEA: "Auth_SEA"
    }
  }
};

/*
 * For easier debugging in development mode, you can import the following file
 * to ignore zone related error stack frames such as `zone.run`, `zoneDelegate.invokeTask`.
 *
 * This import should be commented out in production mode because it will have a negative impact
 * on performance if an error is thrown.
 */
import 'zone.js/plugins/zone-error';  // Included with Angular CLI.
