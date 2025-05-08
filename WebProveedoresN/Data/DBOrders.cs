using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBOrders
    {
        private readonly DBConnection _dBConnection;

        public DBOrders(DBConnection dBConnection)
        {
            _dBConnection = dBConnection;
        }

        public async Task<List<OrderModel>> ListOrdersAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderModel>();
            const string storedProcedure = "sp_GetOrders";
            try
            {
                await using var conexion = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, conexion)
                {
                    CommandType = CommandType.StoredProcedure;
                };

                cmd.Parameters.AddWithValue("@Parameter1", orderDetailDTO.Action);
                cmd.Parameters.AddWithValue("@Parameter2", orderDetailDTO.OrderNumber);
                cmd.Parameters.AddWithValue("@Parameter3", orderDetailDTO.SupplierCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Parameter4", orderDetailDTO.DocumentType ?? (object)DBNull.Value);

                await conexion.OpenAsync();

                await using var reader = cmd.ExecuteReader();
                while (await reader.ReadAsync())
                {
                    orders.Add(new OrderModel()
                    {
                        DocumentType = reader["DocumentType"].ToString(),
                        OrderNumber = reader["OrderNumber"] != DBNull.Value ? (int)reader["OrderNumber"] : 0,
                        OrderDate = reader["OrderDate"] != DBNull.Value ? (DateTime)reader["OrderDate"] : DateTime.MinValue,
                        Canceled = reader["Canceled"].ToString(),
                        TotalAmount = reader["TotalAmount"] != DBNull.Value ? (decimal)reader["TotalAmount"] : 0,
                        IdEstatus = reader["IdEstatus"].ToString(),
                        DocCurOrder = reader["Currency"].ToString() ?? string.Empty,
                        Property = reader["Property"].ToString() ?? string.Empty,
                        Invoices = reader["Invoices"] != DBNull.Value ? (int)reader["Invoices"] : 0,
                        TotalInvoice = reader["TotalInvoice"] != DBNull.Value ? (decimal)reader["TotalInvoice"] : 0,
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al listar órdenes: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al listar órdenes: {ex.Message}", ex);
            }
            return orders;
        }

        public async Task<OrderModel> GetOrderByOrderNumber(int orderNumber)
        {
            var order = new OrderModel();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                conexion.Open();
                var storedProcedure = "sp_GetOrderByOrderNumber";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        order = new OrderModel()
                        {
                            DocumentType = dr["DocumentType"].ToString(),
                            SupplierCode = dr["SupplierCode"].ToString(),
                            OrderNumber = (int)dr["OrderNumber"],
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

        public async Task<OrderModel> GetOrderByOrderNumber2(OrderDetailDTO orderDetailDTO)
        {
            var order = new OrderModel();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                conexion.Open();
                var storedProcedure = "sp_GetOrderByOrderNumber_2";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@Parameter1", orderDetailDTO.Action);
                    cmd.Parameters.AddWithValue("@Parameter2", orderDetailDTO.OrderNumber);
                    cmd.Parameters.AddWithValue("@Parameter3", orderDetailDTO.SupplierCode);
                    cmd.Parameters.AddWithValue("@Parameter4", orderDetailDTO.DocumentType);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    if (dr.Read())
                    {
                        order = new OrderModel()
                        {
                            DocumentType = dr["DocumentType"].ToString(),
                            OrderNumber = (int)dr["OrderNumber"],
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

        public async Task<bool> ValidateOrderNumberInDatabaseAsync(ValidateOrderNumberDTO model)
        {
            bool isValid = false;

            using (SqlConnection connection = await _dBConnection.GetConnectionAsync())
            {
                const string storedProcedure = "sp_ValidateOrderNumber";
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

        public async Task<List<OrderDetailModel>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetailModel>();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
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
                        orders.Add(new OrderDetailModel()
                        {
                            DocumentTypeOrder = dr["TipoDocumento1"].ToString() ?? string.Empty,
                            OrderNumber = dr["NoDocumento1"] != DBNull.Value ? (int)dr["NoDocumento1"] : 0,
                            LineNum = dr["NoLinea1"] != DBNull.Value ? (int)dr["NoLinea1"] : 0,
                            ItemCode = dr["CodigoArticulo1"].ToString() ?? string.Empty,
                            QuantityOrder = dr["Cantidad1"] != DBNull.Value ? (decimal)dr["Cantidad1"] : 0,
                            OpenQty = dr["CantidadPendiente1"] != DBNull.Value ? (decimal)dr["CantidadPendiente1"] : 0,
                            DocCurOrder = dr["Moneda1"].ToString() ?? string.Empty,
                            Tax = dr["Impuesto1"] != DBNull.Value ? (decimal)dr["Impuesto1"] : 0,
                            PricePerItem = dr["PrecioxArticulo1"] != DBNull.Value ? (decimal)dr["PrecioxArticulo1"] : 0,
                            TotalItem = dr["LineaTotal1"] != DBNull.Value ? (decimal)dr["LineaTotal1"] : 0,
                            TotalTaxItem = dr["LineaTotalIVA1"] != DBNull.Value ? (decimal)dr["LineaTotalIVA1"] : 0,
                            TotalOrder = dr["Total1"] != DBNull.Value ? (decimal)dr["Total1"] : 0,
                            DocumentType = dr["TipoDocumento2"].ToString() ?? string.Empty,
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

        public async Task<List<DetailsGoodsReceiptModel>> GetOrderDetailsGoodsReceiptByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<DetailsGoodsReceiptModel>();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
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
                        orders.Add(new DetailsGoodsReceiptModel()
                        {
                            DocNum = dr["NoDocumento2"] != DBNull.Value ? (int)dr["NoDocumento2"] : 0,
                            InvoiceSupplier = dr["FacturaProveedor2"].ToString() ?? string.Empty,
                            ItemCode = dr["CodigoArticulo1"].ToString() ?? string.Empty,
                            OpenQty = dr["CantidadPendiente1"] != DBNull.Value ? (decimal)dr["CantidadPendiente1"] : 0,
                            DocStatus = dr["Status2"].ToString() ?? string.Empty,
                            DocCur = dr["Moneda2"].ToString() ?? string.Empty,
                            Quantity = dr["Cantidad2"] != DBNull.Value ? (decimal)dr["Cantidad2"] : 0,
                            Total = dr["Total2"] != DBNull.Value ? (decimal)dr["Total2"] : 0,
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

        public async Task<List<OrderDetailsOfferModel>> GetOrderDetailsOfferByOrderNumber(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetailsOfferModel>();
            try
            {
                await using var conexion = await _dBConnection.GetConnectionAsync();
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
                        orders.Add(new OrderDetailsOfferModel()
                        {
                            DocumentType = dr["TipoDocumento1"].ToString() ?? string.Empty,
                            OrderNumber = dr["NoDocumento1"] != DBNull.Value ? (int)dr["NoDocumento1"] : 0,
                            OrderDate = dr["Fecha1"] != DBNull.Value ? (DateTime)dr["Fecha1"] : DateTime.MinValue,
                            Canceled = dr["Canceled1"].ToString() ?? string.Empty,
                            Status = dr["Status1"].ToString() ?? string.Empty,
                            TotalAmount = dr["Total1"] != DBNull.Value ? (decimal)dr["Total1"] : 0,
                            DocCurOrder = dr["Moneda1"].ToString() ?? string.Empty,
                            LineNum = dr["NoLinea1"] != DBNull.Value ? (int)dr["NoLinea1"] : 0,
                            ItemCode = dr["CodigoArticulo1"].ToString() ?? string.Empty,
                            Quantity = dr["Cantidad1"] != DBNull.Value ? (decimal)dr["Cantidad1"] : 0,
                            OpenQty = dr["CantidadPendiente1"] != DBNull.Value ? (decimal)dr["CantidadPendiente1"] : 0,
                            Price = dr["PrecioxArticulo1"] != DBNull.Value ? (decimal)dr["PrecioxArticulo1"] : 0,
                            Tax = dr["Impuesto1"] != DBNull.Value ? (decimal)dr["Impuesto1"] : 0,
                            TotalItem = dr["LineaTotal1"] != DBNull.Value ? (decimal)dr["LineaTotal1"] : 0,
                            TotalTax = dr["LineaTotalIVA1"] != DBNull.Value ? (decimal)dr["LineaTotalIVA1"] : 0,
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

        public async Task<List<OrderInvoicesDTO>> GetOrderInvoicesByOrderNumberAsync(int orderNumber)
        {
            var invoices = new List<OrderInvoicesDTO>();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                conexion.Open();
                var storedProcedure = "sp_GetOrderInvoicesByOrderNumber";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        invoices.Add(new OrderInvoicesDTO()
                        {
                            Invoices = dr["Invoices"] != DBNull.Value ? dr["Invoices"].ToString() : string.Empty,
                            TotalInvoice = dr["TotalInvoices"] != DBNull.Value ? (decimal)dr["TotalInvoices"] : 0,
                        });
                    }
                }
                conexion.Close();
            }
            catch (Exception)
            {
                throw;
            }
            return invoices;
        }
    }
}