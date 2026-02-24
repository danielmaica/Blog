using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Blog.Extensions;
using Blog.Models;
using Microsoft.IdentityModel.Tokens;

namespace Blog.Services;

public class TokenService
{
  public string GenerateToken(User user)
  {
    // 1. Pega a chave secreta
    var key = Encoding.ASCII.GetBytes(Configuration.JwtKey);

    // 2. Cria as claims (APENAS em Subject)
    var claims = user.GetClaims();

    // 3. Cria o descriptor do token
    var tokenDescriptor = new SecurityTokenDescriptor
    {
      Subject = new ClaimsIdentity(claims), // ✅ APENAS AQUI!
      Expires = DateTime.UtcNow.AddHours(8),
      SigningCredentials = new SigningCredentials(
        new SymmetricSecurityKey(key),
        SecurityAlgorithms.HmacSha256Signature
      )
    };

    // 4. Gera o token
    var tokenHandler = new JwtSecurityTokenHandler();
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
  }
}
