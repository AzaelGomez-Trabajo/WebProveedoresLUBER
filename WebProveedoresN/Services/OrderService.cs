using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService
    {
        public static List<OrderDTO> GetOrders(string empresa, int parametro1, int parametro2)
        {
            return DBOrders.ListOrders(empresa, parametro1, parametro2);
        }

        public static string ObtenerOrderNumberIdInDatabase(string orderNumber)
        {
            return DBOrders.ObtenerOrderNumberId(orderNumber);
        }

    }
}
