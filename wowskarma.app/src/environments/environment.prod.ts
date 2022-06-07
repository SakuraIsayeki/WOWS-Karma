export const environment = {
  production: true,

  apiHost: {
    EU: "https://api.wows-karma.com/api/",
    NA: "https://api.na.wows-karma.com/api/",
    CIS: "https://api.ru.wows-karma.com/api/",
    SEA: "https://api.asia.wows-karma.com/api/",
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
