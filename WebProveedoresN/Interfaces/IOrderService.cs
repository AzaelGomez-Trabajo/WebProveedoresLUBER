using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderDetail>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO);
        
        Task<List<OrderDetailsOffer>> GetOrderDetailsOfferByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<DetailsGoodsReceipt>> GetOrderDetailsGoodsReceiptByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<Order> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO);

        Task<List<OrderInvoicesDTO>> GetOrderInvoicesByOrderNumberAsync(int orderNumber);

        Task<List<Order>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString);

        Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString);
    }
}