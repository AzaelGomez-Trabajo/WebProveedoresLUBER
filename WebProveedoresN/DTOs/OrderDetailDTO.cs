namespace WebProveedoresN.DTOs
{
    public class OrderDetailDTO
    {
        public int Action { get; set; }

        public int OrderNumber { get; set; }

        public string SupplierCode { get; set; } = null!;

        public string DocumentType { get; set; } = null!;
    }
}
