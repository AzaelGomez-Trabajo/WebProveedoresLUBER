using System.Security.Cryptography;
using System.Text;

namespace WebProveedoresN.Services
{
    public static class UtilityService
    {
        public static string ConvertirSHA256(string texto)
        {

            byte[] hashValue = SHA256.HashData(Encoding.UTF8.GetBytes(texto));

            // Convertir el array byte en cadena de texto
            var hash = new StringBuilder();
            foreach (byte b in hashValue)
                hash.AppendFormat("{0:X2}", b);

            return hash.ToString();

            //var hash = string.Empty;

            //using (var sha256 = SHA256.Create())
            //{
            //    // obtener el hash del texto recibido
            //    byte[] hashValue = sha256.ComputeHash(Encoding.UTF8.GetBytes(texto));

            //    // convertir el array byte en cadena de texto
            //    foreach (byte b in hashValue)
            //        hash += $"{b:X2}";

            //}
            //return hash;
        }

        public static string GenerarToken()
        {
            string token = Guid.NewGuid().ToString("N");
            return token;
        }
    }
}
