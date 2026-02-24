using System.Security.Claims;
using Blog.Models;

namespace Blog.Extensions;

public static class RoleClaimsExtension
{
  public static IEnumerable<Claim> GetClaims(this User user)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new(ClaimTypes.Name, user.Email)
    };

    claims.AddRange(
      user.Roles.Select(role => new Claim(ClaimTypes.Role, role.Slug))
    );

    return claims;
  }
}
