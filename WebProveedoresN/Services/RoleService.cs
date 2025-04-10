using WebProveedoresN.Data;

namespace WebProveedoresN.Services
{
    public class RoleService
    {
        public static List<string> GetRoles() => DBRoles.GetRoles();
    }
}