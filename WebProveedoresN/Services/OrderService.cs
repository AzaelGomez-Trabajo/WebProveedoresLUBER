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

        public async Task<List<OrderDTO>> GetOrdersAsync(string empresa, string searchString, int pageNumber, int pageSize)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(empresa))
            {
                query = query.Where(o => o.SupplierName == empresa);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderNumber.Contains(searchString));
            }

            return await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new OrderDTO
                {
                    OrderNumber = o.OrderNumber,
                    OrderDate = o.OrderDate,
                    TotalAmount = o.TotalAmount,
                    IdEstatus = o.IdEstatus,
                    Currency = o.Currency,
                    Canceled = o.Canceled,
                    Invoices = o.Invoices,
                    TotalInvoice = o.TotalInvoice
                }).ToListAsync();
        }

        public async Task<int> GetTotalOrdersAsync(string empresa, string searchString)
        {
            var query = _context.Orders.AsQueryable();

            if (!string.IsNullOrEmpty(empresa))
            {
                query = query.Where(o => o.SupplierName == empresa);
            }

            if (!string.IsNullOrEmpty(searchString))
            {
                query = query.Where(o => o.OrderNumber.Contains(searchString));
            }

            return await query.CountAsync();
        }


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
