using Microsoft.Build.Framework;

namespace WebProveedoresN.Models
{
    public class StatusDTO
    {
        public int IdStatus { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
