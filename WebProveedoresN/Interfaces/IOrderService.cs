using WebProveedoresN.Entities;
using WebProveedoresN.Models;

namespace WebProveedoresN.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDetail>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<Order> GetOrderByOrderNumberAsync(int orderNumber);

        Task<List<Order>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString, int pageNumber, int pageSize);

        Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString);
    }
}