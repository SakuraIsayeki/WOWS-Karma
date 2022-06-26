export const environment = {
    production: true,

    apiHost: {
        EU: "https://api.wows-karma.com",
        NA: "https://api.na.wows-karma.com",
        CIS: "https://api.ru.wows-karma.com",
        SEA: "https://api.asia.wows-karma.com",
    },

    applicationInsights: {
        connectionString: "InstrumentationKey=98350f18-923b-47ef-a219-57da9f5e6de4;IngestionEndpoint=https://francecentral-0.in.applicationinsights.azure.com/;LiveEndpoint=https://francecentral.livediagnostics.monitor.azure.com/",
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
