using WebProveedoresN.Conexion;
using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService
    {
        public static List<OrderDTO> GetOrders(string empresa)
        {
            return DBOrders.Listar(empresa);
        }

    }
}
