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
            return _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
        }

    }
}
