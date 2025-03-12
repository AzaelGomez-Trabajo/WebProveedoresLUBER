using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public static class StatusService
    {
        public static List<StatusDTO> ObtenerEstatus()
        {
            return DBStatus.ObtenerEstatus();
        }

    }
}
