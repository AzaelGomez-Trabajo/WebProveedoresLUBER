using Microsoft.EntityFrameworkCore;
using WebProveedoresN.Data;
using WebProveedoresN.Entities;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService : IOrderService
    {
        //private readonly DataContext _context;

        //public OrderService(DataContext context)
        //{
        //    _context = context;
        //}

        public async Task<List<OrderDTO>> GetOrdersAsync(string supplierCode, int action, int orderNumber, string DocumentType, string searchString, int pageNumber, int pageSize)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(supplierCode, action, orderNumber, DocumentType);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber!.Contains(searchString)).ToList();
            }

            // Aplicar la paginación en memoria
            var paginatedOrders = orders
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return await Task.FromResult(paginatedOrders);
        }

        public async Task<int> GetTotalOrdersAsync(string supplierCode, int parameter1, int parameter2, string parameter4, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(supplierCode, parameter1, parameter2, parameter4);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber!.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders.Count);
        }

        public async Task<List<OrderDetail>> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
              var orderDetail = DBOrders.GetOrderByOrderNumber(orderDetailDTO);
            if (orderDetail == null)
            {
                return null!;
            }
            return await Task.FromResult(orderDetail);
        }
    }
}