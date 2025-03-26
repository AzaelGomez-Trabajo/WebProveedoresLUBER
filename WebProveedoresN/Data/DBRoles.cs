using Microsoft.Data.SqlClient;
using System.Data;

namespace WebProveedoresN.Data
{
    public class DBRoles
    {
        public static List<string> ObtenerRoles()
        {
            var roles = new List<string>();
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ListarRoles";
                using var cmd = new SqlCommand(storedProcedure, conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    roles.Add(dr["Name"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            return roles;
        }

    }
}
