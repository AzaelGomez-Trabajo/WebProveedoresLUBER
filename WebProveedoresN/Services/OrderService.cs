using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Interfaces;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public class OrderService : IOrderService
    {
        public async Task<List<Order>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(orderDetailDTO);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber.ToString()!.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders);
        }

        public async Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = DBOrders.ListOrders(orderDetailDTO);

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber.ToString()!.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders.Count);
        }

        public async Task<Order> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var order = DBOrders.GetOrderByOrderNumber2(orderDetailDTO);
            // Obtener todos los registros desde la base de datos
            //var order = DBOrders.GetOrderByOrderNumber(orderDetailDTO.OrderNumber);
            if (order == null)
            {
                return null!;
            }
            return await Task.FromResult(order);
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

        public async Task<List<OrderDetailsOffer>> GetOrderDetailsOfferByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orderDetail = DBOrders.GetOrderDetailsOfferByOrderNumber(orderDetailDTO);
            if (orderDetail == null)
            {
                return null!;
            }
            return await Task.FromResult(orderDetail);
        }

        public async Task<List<DetailsGoodsReceipt>> GetOrderDetailsGoodsReceiptByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orderDetail = DBOrders.GetOrderDetailsGoodsReceiptByOrderNumber(orderDetailDTO);
            if (orderDetail == null)
            {
                return null!;
            }
            return await Task.FromResult(orderDetail);
        }

        public async Task<List<OrderInvoicesDTO>> GetOrderInvoicesByOrderNumberAsync(int orderNumber)
        {
            var orderInvoices = DBOrders.GetOrderInvoicesByOrderNumber(orderNumber);
            if (orderInvoices == null)
            {
                return null!;
            }
            return await Task.FromResult(orderInvoices);
        }
    }
}