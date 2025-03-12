using Microsoft.Data.SqlClient;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBAcceso
    {
        public static List<AccesoModel> ObtenerAccesos()
        {
            var accesos = new List<AccesoModel>();
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var query = "SELECT * FROM Acceso";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            accesos.Add(new AccesoModel
                            {
                                IdAcceso = Convert.ToInt32(dr["IdAcceso"]),
                                Acceso = dr["Acceso"].ToString()
                            });
                        }
                    }
                }
            }
            return accesos;
        }
    }
}
