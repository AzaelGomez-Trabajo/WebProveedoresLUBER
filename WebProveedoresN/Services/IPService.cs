using WebProveedoresN.Interfaces;
using Microsoft.AspNetCore.Http;

namespace WebProveedoresN.Services
{
    public class IPService : IIPService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserIpAddress()
        {
            var ipAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            if (ipAddress == "::1" || string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = "127.0.0.1"; // Asignar la dirección IPv4 de loopback si es local
            }
            return ipAddress;
        }
    }
}