namespace WebProveedoresN.Models
{
    public class UsuarioModel
    {
        public int Id { get; set; }
        public string? Empresa { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Clave { get; set; }
        public string? ConfirmarClave { get; set; }
        public bool Restablecer { get; set; }
        public bool Confirmado { get; set; }
        public string? Token { get; set; }
        public int? IdAcceso { get; set; }
        public int? IdState { get; set; }

    }
}
