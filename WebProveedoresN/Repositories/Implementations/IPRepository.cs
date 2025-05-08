using Microsoft.AspNetCore.Http;
using WebProveedoresN.Repositories.Interfaces;

namespace WebProveedoresN.Repositories.Implementations
{
    public class IPRepository : IIPRepository
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IPRepository(IHttpContextAccessor httpContextAccessor)
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