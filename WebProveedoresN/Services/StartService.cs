using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Services
{
    public static class StartService
    {
        public static bool ValidateFirstUser(Supplier supplier) => DBStart.ValidateFirstUser(supplier);

        public static bool ValidateSupplier(Supplier supplier) => DBStart.ValidateSupplier(supplier);

        public static string SaveUserWithRoles(Usuario usuario) => DBStart.SaveUserWithRoles(usuario);

        public static string ConfirmEmail(string token) => DBStart.ConfirmEmail(token);

        public static Usuario ValidateUser(string correo, string clave) => DBStart.ValidateUser(correo, clave);
    }
}
