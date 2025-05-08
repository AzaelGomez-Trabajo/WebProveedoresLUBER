using Microsoft.Data.SqlClient;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBStatus
    {
        public static List<StatusModel> GetStatus()
        {
            var status = new List<StatusModel>();
            using (var conexion = DBConnection.GetConnection())
            {
                conexion.Open();
                var query = "SELECT Id, Name FROM Status";
                using (var cmd = new SqlCommand(query, conexion))
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        while (dr.Read())
                        {
                            status.Add(new StatusModel
                            {
                                IdStatus = Convert.ToInt32(dr["Id"]),
                                Status = dr["Name"].ToString()
                            });
                        }
                    }
                }
            }
            return status;
        }

    }
}
