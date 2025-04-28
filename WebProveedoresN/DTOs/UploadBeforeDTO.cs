namespace WebProveedoresN.DTOs
{
    public class UploadBeforeDTO 
    {
        public int OrderNumber { get; set; }

        public string DocumentType { get; set; } = string.Empty;

        public List<string> Items { get; set; } = new List<string>();
    }
}
