using Microsoft.EntityFrameworkCore;
using WebProveedoresN.Data;
using WebProveedoresN.Entities;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService : IOrderService
    {
        public async Task<List<Order>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString, int pageNumber, int pageSize)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(orderDetailDTO);

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

        public async Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(orderDetailDTO);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber!.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders.Count);
        }

        public async Task<List<OrderDetail>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
              var orderDetail = DBOrders.GetOrderDetailsByOrderNumber(orderDetailDTO);
            if (orderDetail == null)
            {
                return null!;
            }
            return await Task.FromResult(orderDetail);
        }

        public async Task<Order> GetOrderByOrderNumberAsync(int orderNumber)
        {
            var order = DBOrders.GetOrderNumber(orderNumber.ToString());
            if (order == null)
            {
                return null!;
            }
            return await Task.FromResult(order);
        }
    }
}