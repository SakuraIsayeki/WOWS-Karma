using Microsoft.AspNetCore.Authentication;

namespace framework.jwt
{
  public class JwtOptions : AuthenticationSchemeOptions
  {
    public string ClientId { get; set; }

    public bool SaveToken { get; set; }
  }
}
