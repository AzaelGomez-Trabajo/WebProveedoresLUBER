namespace WebProveedoresN.DTOs
{
    public class OrderDetailsDTO
    {
        public int OrderNumber { get; set; }

        public string SupplierCode { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;
    }
}
