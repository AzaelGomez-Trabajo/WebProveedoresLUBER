using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBOrders
    {
        public static List<OrderDTO> ListOrders(string empresa, int parametro1, int parametro2)
        {
            var orders = new List<OrderDTO>();
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ListarOrdenes";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@Parameter1", parametro1);
                    cmd.Parameters.AddWithValue("@Parameter2", parametro2);
                    cmd.Parameters.AddWithValue("@Parameter3", empresa);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new OrderDTO()
                        {
                            OrderNumber = dr["OrderNumber"].ToString(),
                            OrderDate = (DateTime)dr["OrderDate"],
                            Canceled = dr["Canceled"].ToString(),
                            TotalAmount = (decimal)dr["TotalAmount"],
                            Status = dr["IdEstatus"].ToString(),
                            Currency = dr["Currency"].ToString(),
                            Invoices = (int)dr["Invoices"],
                            TotalInvoices = (decimal)dr["TotalInvoice"],
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

        public static string ObtenerOrderNumberId(string orderNumber)
        {
            var orderNumberId = string.Empty;
            using (var connection = DBConexion.ObtenerConexion())
            {
                string query = "SELECT IdOrder FROM Orders WHERE OrderNumber = @OrderNumber";
                using (var cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.CommandType = CommandType.Text;
                    connection.Open();
                    orderNumberId = cmd.ExecuteScalar().ToString();
                    connection.Close();
                }
            }
            return orderNumberId;
        }


    }
}
