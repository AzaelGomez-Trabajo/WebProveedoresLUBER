using WebProveedoresN.Data;

namespace WebProveedoresN.Repositories.Implementations
{
    public class RolesRepository
    {
        public static List<string> GetRoles() => DBRoles.GetRoles();
    }
}