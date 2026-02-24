namespace Blog;

public static class Configuration
{
  public static string JwtKey { get; set; } = "";
  public static string ApiKeyName { get; set; } = "";
  public static string ApiKey { get; set; } = "";
  public static SmtpConfiguration Smtp { get; set; } = new();

  public class SmtpConfiguration
  {
    // sendgrid-apikey = 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
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
  }
}