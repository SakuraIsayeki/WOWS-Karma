export const environment = {
  production: true,

  apiHost: {
    EU: "https://api.wows-karma.com",
    NA: "https://api.na.wows-karma.com",
    CIS: "https://api.ru.wows-karma.com",
    SEA: "https://api.asia.wows-karma.com",
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
