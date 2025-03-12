using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public static class AccesoService
    {
        public static List<AccesoModel> ObtenerAccesos()
        {
            return DBAcceso.ObtenerAccesos();
        }
    }
}
