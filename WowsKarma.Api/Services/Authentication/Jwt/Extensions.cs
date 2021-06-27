using framework.jwt;
using Microsoft.AspNetCore.Authentication;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
  public static class Extensions
  {
    public static AuthenticationBuilder AddCustomJwt(this AuthenticationBuilder builder, Action<JwtOptions> action)
    {
      builder.Services.AddSingleton<JwtService>();
      builder.AddScheme<JwtOptions, JwtHandler>("Bearer", action);

      return builder;
    }
  }
}
