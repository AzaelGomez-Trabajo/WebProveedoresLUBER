using WebProveedoresN.Models;

namespace WebProveedoresN.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetOrdersAsync(string empresa, string searchString, int pageNumber, int pageSize);
        Task<int> GetTotalOrdersAsync(string empresa, string searchString);
    }
}
