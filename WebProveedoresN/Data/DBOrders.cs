using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBOrders
    {
        public static List<Order> ListOrders(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<Order>();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_GetOrders";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@Parameter1", orderDetailDTO.Action);
                    cmd.Parameters.AddWithValue("@Parameter2", orderDetailDTO.OrderNumber);
                    cmd.Parameters.AddWithValue("@Parameter3", orderDetailDTO.SupplierCode);
                    cmd.Parameters.AddWithValue("@Parameter4", orderDetailDTO.DocumentType);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        orders.Add(new Order()
                        {
                            DocumentType = dr["DocumentType"].ToString(),
                            OrderNumber = dr["OrderNumber"].ToString(),
                            OrderDate = (DateTime)dr["OrderDate"],
                            Canceled = dr["Canceled"].ToString(),
                            TotalAmount = (decimal)dr["TotalAmount"],
                            IdEstatus = dr["IdEstatus"].ToString(),
                            DocCurOrder = dr["Currency"].ToString()!,
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

        public static Order GetOrderNumber(string orderNumber)
        {
            var order = new Order();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_GetOrderByOrderNumber";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        order = new Order()
                        {
                            DocumentType = dr["DocumentType"].ToString(),
                            OrderNumber = dr["OrderNumber"].ToString(),
                            OrderDate = (DateTime)dr["OrderDate"],
                            Canceled = dr["Canceled"].ToString(),
                            IdEstatus = dr["IdEstatus"].ToString(),
                            TotalAmount = (decimal)dr["TotalAmount"],
                            DocCurOrder = dr["Currency"].ToString()!,
                            Property = dr["Property"].ToString() ?? string.Empty,
                            Invoices = (int)dr["Invoices"],
                            TotalInvoice = (decimal)dr["TotalInvoice"],
                        };
                    }
                }
                conexion.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return order;
        }

        public static bool ValidateOrderNumberInDatabase(Order model)
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

        public static List<OrderDetail> GetOrderDetailsByOrderNumber(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetail>();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_GetOrderDetailsByOrderNumber";
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
                            DocumentTypeOrder = dr["Tipo Documento 1"].ToString() ?? string.Empty,
                            OrderNumber = dr["DocNum1"].ToString() ?? string.Empty,
                            LineNum = dr["LineNum1"] != DBNull.Value ? (int)dr["LineNum1"] : 0,
                            ItemCode = dr["ItemCode1"].ToString() ?? string.Empty,
                            QuantityOrder = dr["Quantity1"] != DBNull.Value ? (decimal)dr["Quantity1"] : 0,
                            OpenQty = dr["OpenQty1"] != DBNull.Value ? (decimal)dr["OpenQty1"] : 0,
                            DocCurOrder = dr["DocCur1"].ToString() ?? string.Empty,
                            TotalOrder = dr["Total1"] != DBNull.Value ? (decimal)dr["Total1"] : 0,
                            DocumentType = dr["Tipo Documento 2"].ToString() ?? string.Empty,
                            DocNum = dr["DocNum"] != DBNull.Value ? (int)dr["DocNum"] : 0,
                            CardCode = dr["CardCode"].ToString() ?? string.Empty,
                            CardName = dr["CardName"].ToString() ?? string.Empty,
                            DocStatus = dr["DocStatus"].ToString() ?? string.Empty,
                            DocCur = dr["DocCur"].ToString() ?? string.Empty,
                            Total = dr["Total"] != DBNull.Value ? (decimal)dr["Total"] : 0,
                            Quantity = dr["Quantity"] != DBNull.Value ? (decimal)dr["Quantity"] : 0,
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