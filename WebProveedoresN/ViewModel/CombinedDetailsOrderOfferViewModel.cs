using WebProveedoresN.Models;

namespace WebProveedoresN.ViewModel
{
    public class CombinedDetailsOrderOfferViewModel
    {
        public OrderModel? Orders { get; set; }
        public List<OrderDetailsOfferModel>? OrderDetailsOffer { get; set; }
    }
}
