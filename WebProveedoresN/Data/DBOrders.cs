using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Entities;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBOrders
    {
        public static List<OrderDTO> ListOrders(string supplierCode, int action, int orderNumber, string documentType)
        {
            var orders = new List<OrderDTO>();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_GetOrders";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@Parameter1", action);
                    cmd.Parameters.AddWithValue("@Parameter2", orderNumber);
                    cmd.Parameters.AddWithValue("@Parameter3", supplierCode);
                    cmd.Parameters.AddWithValue("@Parameter4", documentType);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new OrderDTO()
                        {
                            DocumentType = dr["DocumentType"].ToString(),
                            OrderNumber = dr["OrderNumber"].ToString(),
                            OrderDate = (DateTime)dr["OrderDate"],
                            Canceled = dr["Canceled"].ToString(),
                            TotalAmount = (decimal)dr["TotalAmount"],
                            IdEstatus = dr["IdEstatus"].ToString(),
                            Currency = dr["Currency"].ToString(),
                            Property = dr["Property"].ToString(),
                            Invoices = (int)dr["Invoices"],
                            TotalInvoice = (decimal)dr["TotalInvoice"],
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

            using (SqlConnection connection = DBConnectiion.GetConnection())
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

        public static string ObtenerOrderNumber(string orderNumber)
        {
            var orderNumberId = string.Empty;
            using (var connection = DBConnectiion.GetConnection())
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

        public static List<OrderDetail> GetOrderByOrderNumber(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetail>();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_GetOrderByOrderNumber";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@Action", orderDetailDTO.Action);
                    cmd.Parameters.AddWithValue("@OrderNumber", orderDetailDTO.OrderNumber);
                    cmd.Parameters.AddWithValue("@SupplierCode", orderDetailDTO.SupplierCode);
                    cmd.Parameters.AddWithValue("@DocumentType", orderDetailDTO.DocumentType);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new OrderDetail()
                        {

                            DocumentTypeOrder = dr["Tipo Documento"].ToString()!,
                            OrderNumber = (int)dr["DocNum"],
                            LineNum = (int)dr["LineNum"],
                            ItemCode = dr["ItemCode"].ToString()!,
                            QuantityOrder = (decimal)dr["Quantity"],
                            OpenQty = (decimal)dr["OpenQty"],
                            DocCurOrder = dr["DocCur"].ToString()!,
                            TotalOrder = (decimal)dr["Total"],
                            DocumentType = dr["Tipo Documento"].ToString(),
                            DocNum = (int)dr["DocNum"],
                            CardCode = dr["CardCode"].ToString(),
                            CardName = dr["CardName"].ToString(),
                            DocStatus = dr["DocStatus"].ToString(),
                            DocCur = dr["DocCur"].ToString(),
                            Total = (decimal)dr["Total"],
                            Quantity = (decimal)dr["Quantity"],
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
    }
}