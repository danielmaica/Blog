using System.Text.RegularExpressions;
using Blog.Data;
using Blog.Extensions;
using Blog.Models;
using Blog.Services;
using Blog.ViewModels;
using Blog.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureIdentity.Password;

namespace Blog.Controllers;

[ApiController]
[Route("v1/account")]
public class AccountController : ControllerBase
{
  private readonly BlogDataContext _context = new();
  private readonly TokenService _tokenService = new();
  private readonly EmailService _emailService = new();

  public AccountController(BlogDataContext context, TokenService tokenService, EmailService emailService)
  {
    _context = context;
    _tokenService = tokenService;
    _emailService = emailService;
  }

  [HttpPost("register")]
  public async Task<IActionResult> Register(RegisterViewModel model)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

    var user = new User
    {
      Name = model.Name,
      Email = model.Email,
      Image = model.Image,
      Slug = model.Email.Replace("@", "-").Replace(".", "-"),
      Bio = model.Bio
    };

    var password = PasswordGenerator.Generate(25);
    user.PasswordHash = PasswordHasher.Hash(password);

    try
    {
      await _context.Users.AddAsync(user);
      await _context.SaveChangesAsync();

      _emailService.Send(
        user.Name,
        user.Email,
        "Blog: cadastro de conta",
        $"Parabéns {user.Name}, sua conta foi cadastrada com sucesso, a senha de acesso é <strong>{password}</strong>.");

      return Ok(new ResultViewModel<dynamic>(new
      {
        user.Email,
        password
      }));
    }
    catch (DbUpdateException)
    {
      return StatusCode(400, new ResultViewModel<string>("05EX06 - Este e-mail já está em uso."));
    }
  }

  [HttpPost("login")]
  public async Task<IActionResult> Login([FromBody] LoginViewModel model)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

    var user = await _context.Users
      .AsNoTracking()
      .Include(u => u.Roles)
      .FirstOrDefaultAsync(u => u.Email == model.Email);

    if (user == null || !PasswordHasher.Verify(user.PasswordHash, model.Password))
      return StatusCode(401, new ResultViewModel<string>("Usuário ou senha inválidos."));

    try
    {
      var token = _tokenService.GenerateToken(user);
      return Ok(new ResultViewModel<string>(token, null));
    }
    catch (DbUpdateException)
    {
      return StatusCode(400, new ResultViewModel<string>("05EX07 - Erro ao gerar o token."));
    }
  }

  [Authorize]
  [HttpPost("upload-image")]
  public async Task<IActionResult> UploadImage([FromBody] UploadImageViewModel model)
  {
    if (!ModelState.IsValid)
      return BadRequest(new ResultViewModel<string>(ModelState.GetErrors()));

    var fileName = $"{Guid.NewGuid().ToString()}.jpg";
    var data = new Regex(@"data:image\/[a-z]+;base64,").Replace(model.Base64Image, "");
    var bytes = Convert.FromBase64String(data);

    try
    {
      await System.IO.File.WriteAllBytesAsync($"wwwroot/images/{fileName}", bytes);
    }
    catch (DbUpdateException)
    {
      return StatusCode(400, new ResultViewModel<string>("05EX08 - Erro ao fazer upload da imagem."));
    }

    var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == User.Identity!.Name);

    if (user == null)
      return NotFound(new ResultViewModel<string>("05EX09 - Usuário não encontrado."));

    user.Image = $"https://localhost:0000/images/{fileName}";

    try
    {
      _context.Users.Update(user);
      await _context.SaveChangesAsync();
    }
    catch (Exception)
    {
      return StatusCode(500, new ResultViewModel<string>("05EX10 - Erro ao salvar imagem no cadastro do usuário."));
    }

    return Ok(new ResultViewModel<string>("Upload de imagem realizado com sucesso.", null));
  }
}
