using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBOrders
    {
        public static List<OrderDTO> ListOrders(string empresa)
        {
            var orders = new List<OrderDTO>();
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ListarOrdenes";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@SupplierName", empresa);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new OrderDTO()
                        {
                            Id = Convert.ToInt32(dr["IdOrder"]),
                            OrderNumber = dr["OrderNumber"].ToString(),
                            OrderDate = (DateTime)dr["OrderDate"],
                            TotalAmount = (decimal)dr["TotalAmount"],
                            Status = dr["Status"].ToString()
                        });
                    }
                 
                }
                conexion.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return orders;
        }

        public static bool ValidateOrderNumberInDatabase(OrderDTO model)
        {
            bool isValid = false;

            using (SqlConnection connection = DBConexion.ObtenerConexion())
            {
                string storedProcedure = "sp_ValidateOrderNumber";
                using (var cmd = new SqlCommand(storedProcedure, connection))
                {
                    cmd.Parameters.AddWithValue("@NumeroOrden", model.OrderNumber);
                    cmd.Parameters.AddWithValue("@Proveedor", model.SupplierName);
                    cmd.CommandType = CommandType.StoredProcedure;

                    connection.Open();
                    Console.WriteLine($"Ejecutando procedimiento almacenado para NumeroOrden: {model.OrderNumber}, Proveedor: {model.SupplierName}");
                    int count = (int)cmd.ExecuteScalar();
                    Console.WriteLine($"Este es el resultado del SP: {count}");
                    isValid = count > 0;
                    connection.Close();
                }
            }
            return isValid;
        }



    }
}
