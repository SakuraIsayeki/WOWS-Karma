using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;

namespace framework.jwt
{
  public class JwtResultContext : ResultContext<JwtOptions>
  {
    public JwtResultContext(HttpContext context, AuthenticationScheme scheme, JwtOptions options) : base(context, scheme, options)
    {
    }
  }
}
