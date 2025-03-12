using Microsoft.Data.SqlClient;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBStatus
    {
        public static List<StatusModel> ObtenerEstatus()
        {
            var status = new List<StatusModel>();
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var query = "SELECT * FROM cStatus";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            status.Add(new StatusModel
                            {
                                IdStatus = Convert.ToInt32(dr["IdStatus"]),
                                Status = dr["Status"].ToString()
                            });
                        }
                    }
                }
            }
            return status;
        }

    }
}
