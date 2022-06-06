/**
 *  @type {import('next').NextConfig}
 */

module.exports = {
    target: 'serverless',

    async rewrites() {
        return [
            // Rewrite everything to `pages/index`
            {
                source: "/:any*",
                destination: "/"
            }
        ];
    },

    experimental: {
        outputStandalone: true
    },

    /*
    const publicRuntimeConfig = {
        API_HOST: isDev ? {
            EU: 'http://localhost:5010/',
            NA: 'http://localhost:5010/',
            CIS: 'http://localhost:5010/',
            SEA: 'http://localhost:5010/'
        } : {
            EU: 'https://api.wows-karma.com/api/',
            NA: 'https://api.na.wows-karma.com/api/',
            CIS: 'https://api.ru.wows-karma.com/api/',
            SEA: 'https://api.asia.wows-karma.com/api/'
        },

        COOKIE_NAME: {
            EU: 'Auth_EU',
            NA: 'Auth_NA',
            CIS: 'Auth_CIS',
            SEA: 'Auth_SEA'
        },

        COOKIE_DOMAIN: isDev ? 'localhost' : '.wows-karma.com',

        APPLICATION_INSIGHTS_CONNECTION_STRING: 'InstrumentationKey=98350f18-923b-47ef-a219-57da9f5e6de4;IngestionEndpoint=https://francecentral-0.in.applicationinsights.azure.com/'
    }
    */


    env: {
        API_HOST_EU: process.env.NEXT_PUBLIC_API_HOST_EU,
        API_HOST_NA: process.env.NEXT_PUBLIC_API_HOST_NA,
        API_HOST_CIS: process.env.NEXT_PUBLIC_API_HOST_CIS,
        API_HOST_SEA: process.env.NEXT_PUBLIC_API_HOST_SEA,

        COOKIE_NAME_EU: process.env.NEXT_PUBLIC_COOKIE_NAME_EU,
        COOKIE_NAME_NA: process.env.NEXT_PUBLIC_COOKIE_NAME_NA,
        COOKIE_NAME_CIS: process.env.NEXT_PUBLIC_COOKIE_NAME_CIS,
        COOKIE_NAME_SEA: process.env.NEXT_PUBLIC_COOKIE_NAME_SEA,

        COOKIE_DOMAIN: process.env.NEXT_PUBLIC_COOKIE_DOMAIN,

        APPLICATION_INSIGHTS_CONNECTION_STRING: process.env.NEXT_PUBLIC_APPLICATION_INSIGHTS_CONNECTION_STRING
    },

    useStrict: true,
    disablePoweredByHeader: true
}
