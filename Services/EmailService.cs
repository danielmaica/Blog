using System.Net;
using System.Net.Mail;

namespace Blog.Services
{
  public class EmailService
  {
    public bool Send(
        string toName,
        string toEmail,
        string subject,
        string body,
        string fromName = "Daniel",
        string fromEmail = "danielmaica.dev@gmail.com"
    )
    {
      var smtpClient = new SmtpClient(Configuration.Smtp.Host, Configuration.Smtp.Port)
      {
        Credentials = new NetworkCredential(Configuration.Smtp.UserName, Configuration.Smtp.Password),
        DeliveryMethod = SmtpDeliveryMethod.Network,
        EnableSsl = true
      };

      var mail = new MailMessage();
      mail.From = new MailAddress(fromEmail, fromName);
      mail.To.Add(new MailAddress(toEmail, toName));
      mail.Subject = subject;
      mail.Body = body;
      mail.IsBodyHtml = true;

      try
      {
        smtpClient.Send(mail);
        return true;
      }
      catch (Exception e)
      {
        var error = e;
        return false;
      }
    }
  }
}