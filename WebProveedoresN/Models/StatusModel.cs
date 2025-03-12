using Microsoft.Build.Framework;

namespace WebProveedoresN.Models
{
    public class StatusModel
    {
        public int IdStatus { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
