export const environment = {
    name: "production",

    apiHost: {
        EU: "https://api.wows-karma.com",
        NA: "https://api.na.wows-karma.com",
        CIS: "https://api.ru.wows-karma.com",
        SEA: "https://api.asia.wows-karma.com",
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
