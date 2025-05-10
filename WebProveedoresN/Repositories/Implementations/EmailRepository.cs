using MimeKit;
using WebProveedoresN.Models;
using MailKit.Net.Smtp;
using WebProveedoresN.Repositories.Interfaces;


namespace WebProveedoresN.Repositories.Implementations
{
    public class EmailRepository : IEmailRepository
    {
        public bool SendEmail(EmailModel email, string name)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("LUBER Lubricantes", "noeazaelgomez@gmail.com"));
                message.To.Add(new MailboxAddress("", email.Para));

                if (!string.IsNullOrEmpty(email.CCO))
                {
                    message.Bcc.Add(new MailboxAddress("", email.CCO));
                }

                message.Subject = email.Asunto;
                message.Body = new TextPart("html")
                {
                    Text = email.Contenido
                };

                using (var client = new SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, false);
                    client.Authenticate("noeazaelgomez@gmail.com", "ubuavdxilsaygnev");
                    client.Send(message);
                    client.Disconnect(true);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo: {ex.Message}");
                return false;
            }
        }
    }
}