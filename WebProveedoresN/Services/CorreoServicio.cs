using MimeKit;
using WebProveedoresN.Models;
using MailKit.Net.Smtp;
using System.Security.Claims;


namespace WebProveedoresN.Services
{
    public static class CorreoServicio
    {
        public static void EnviarCorreo(CorreoDTO correo, string nombre)
        {

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("LUBER Lubricantes", "noeazaelgomez@gmail.com"));
            message.To.Add(new MailboxAddress("", correo.Para));

            if (!string.IsNullOrEmpty(correo.CCO))
            {
                message.Bcc.Add(new MailboxAddress("", correo.CCO));
            }

            message.Subject = correo.Asunto;
            message.Body = new TextPart("html")
            {
                Text = correo.Contenido
            };

            using (var client = new SmtpClient())
            {
                client.Connect("smtp.gmail.com", 587, false);
                client.Authenticate("noeazaelgomez@gmail.com", "ubuavdxilsaygnev");
                client.Send(message);
                client.Disconnect(true);
            }
        }
    }
}