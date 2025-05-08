namespace WebProveedoresN.DTOs
{
    public class GetUserDTO
    {
        public string FullName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool Restablecer { get; set; }
        public bool Confirmado { get; set; }
        public string Token { get; set; } = null!;
    }
}
