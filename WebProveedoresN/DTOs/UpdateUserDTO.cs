namespace WebProveedoresN.DTOs
{
    public class UpdateUserDTO
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int StatusId { get; set; }
        public string Token { get; set; } = null!;
    }
}
