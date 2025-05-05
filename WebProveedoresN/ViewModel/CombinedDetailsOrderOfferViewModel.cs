using WebProveedoresN.Models;

namespace WebProveedoresN.ViewModel
{
    public class CombinedDetailsOrderOfferViewModel
    {
        public Order? Orders { get; set; }
        public List<OrderDetailsOffer>? OrderDetailsOffer { get; set; }
    }
}
