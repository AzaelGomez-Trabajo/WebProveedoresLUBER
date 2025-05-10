using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<List<OrderModel>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString);

        Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString);

        Task<OrderModel> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<OrderDetailModel>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO);
        
        Task<List<DetailsGoodsReceiptModel>> GetOrderDetailsGoodsReceiptByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<OrderDetailsOfferModel>> GetOrderDetailsOfferByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<OrderInvoicesDTO>> GetOrderInvoicesByOrderNumberAsync(int orderNumber);
    }
}