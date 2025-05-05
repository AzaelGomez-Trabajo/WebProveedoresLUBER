using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.ViewModel
{
    public class CombinedDetailsOrderViewModel
    {
        public Order? Orders { get; set; }
        public List<OrderDetail>? OrderDetails { get; set; }
        public List<DetailsGoodsReceipt>? OrderDetailsGoodsReceipt { get; set; }
        public List<OrderInvoicesDTO>? OrderInvoices { get; set; }
    }
}
