using Microsoft.EntityFrameworkCore;
using WebProveedoresN.Data;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService : IOrderService
    {
        private readonly DataContext _context;

        public OrderService(DataContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDTO>> GetOrdersAsync(string supplierCode, int parameter1, int parameter2, string searchString, int pageNumber, int pageSize)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(supplierCode, parameter1, parameter2);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber.Contains(searchString)).ToList();
            }

            // Aplicar la paginación en memoria
            var paginatedOrders = orders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(paginatedOrders);
        }

        public async Task<int> GetTotalOrdersAsync(string supplierCode, int parameter1, int parameter2, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(supplierCode, parameter1, parameter2);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders.Count);
        }


        public static List<OrderDTO> GetOrders(string empresa, int parametro1, int parametro2) => DBOrders.ListOrders(empresa, parametro1, parametro2);
        
        public static string ObtenerOrderNumberIdInDatabase(string orderNumber) => DBOrders.ObtenerOrderNumberId(orderNumber);

    }
}
