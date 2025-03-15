using WebProveedoresN.Conexion;
using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService
    {
        public static List<OrderDTO> GetOrders(string empresa)
        {
            return DBOrders.ListOrders(empresa);
        }

        public static string ObtenerOrderNumberIdInDatabase(string orderNumber)
        {
            return DBOrders.ObtenerOrderNumberId(orderNumber);
        }

    }
}
