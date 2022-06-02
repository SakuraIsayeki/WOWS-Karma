/**
 *  @type {import('next').NextConfig}
 */

const {
    PHASE_DEVELOPMENT_SERVER,
    PHASE_PRODUCTION_BUILD,
} = require('next/constants')

module.exports = (phase) => {
    // when started in development mode `next dev` or `npm run dev` regardless of the value of STAGING environmental variable
    const isDev = phase === PHASE_DEVELOPMENT_SERVER
    // when `next build` or `npm run build` is used
    const isProd = phase === PHASE_PRODUCTION_BUILD

    console.log(`isDev:${isDev}  isProd:${isProd}`)

    const env = {
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

    return {
        env,
        useStrict: true,
        disablePoweredByHeader: true
    }
}
