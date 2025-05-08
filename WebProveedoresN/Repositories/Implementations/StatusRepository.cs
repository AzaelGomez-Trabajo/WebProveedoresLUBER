using WebProveedoresN.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Implementations
{
    public static class StatusRepository
    {
        public static List<StatusModel> GetStatus() => DBStatus.GetStatus();
    }
}