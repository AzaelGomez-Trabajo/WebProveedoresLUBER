using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.ViewModel
{
    public class CombinedDetailsOrderViewModel
    {
        public OrderModel? Orders { get; set; }
        public List<OrderDetailModel>? OrderDetails { get; set; }
        public List<DetailsGoodsReceiptModel>? OrderDetailsGoodsReceipt { get; set; }
        public List<OrderInvoicesDTO>? OrderInvoices { get; set; }
    }
}
