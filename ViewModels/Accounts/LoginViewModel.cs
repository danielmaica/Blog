using System.ComponentModel.DataAnnotations;

namespace Blog.ViewModels.Accounts;

public class LoginViewModel
{
  [Required(ErrorMessage = "O email é obrigatório.")]
  [EmailAddress(ErrorMessage = "O email é inválido.")]
  public string Email { get; set; } = "";

  public string Password { get; set; } = "";
}
