using System.Net;
using System.Net.Mail;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public static class CorreoServicio
    {
        public static void EnviarCorreo(CorreoDTO correo)
        {
            var fromAddress = new MailAddress("noeazaelgomez@gmail.com", "LUBER Lubricantes");
            var toAddress = new MailAddress(correo.Para, correo.CCO);
            const string fromPassword = "ubuavdxilsaygnev";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = correo.Asunto,
                Body = correo.Contenido,
                IsBodyHtml = true
            };
            smtp.Send(message);

        }
    }
}