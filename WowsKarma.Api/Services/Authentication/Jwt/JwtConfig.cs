using System.Collections.Generic;

namespace framework.jwt
{
  public class JwtConfig
  {
    public int ClockSkew { get; set; } = 300;

    public Dictionary<string, JwtConfigClient> Clients { get; set; }
  }

  public class JwtConfigClient
  {
    public string Secret { get; set; }
    public string Audience { get; set; }
  }
}
