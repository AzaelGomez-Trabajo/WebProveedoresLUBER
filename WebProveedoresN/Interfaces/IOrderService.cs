using WebProveedoresN.Entities;
using WebProveedoresN.Models;

namespace WebProveedoresN.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDetail>> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<OrderDTO>> GetOrdersAsync(string SupplierCode, int action, int orderNumber, string DocumentType, string searchString, int pageNumber, int pageSize);

        Task<int> GetTotalOrdersAsync(string empresa, int parametro1, int parametro2, string parameter4, string searchString);
    }
}