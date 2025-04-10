using WebProveedoresN.Models;

namespace WebProveedoresN.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDTO>> GetOrdersAsync(string empresa, int parametro1, int parametro2, string searchString, int pageNumber, int pageSize);

        Task<int> GetTotalOrdersAsync(string empresa, int parametro1, int parametro2, string searchString);
    }
}