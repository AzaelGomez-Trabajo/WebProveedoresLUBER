using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;
using WebProveedoresN.Repositories.Interfaces;

namespace WebProveedoresN.Repositories.Implementations
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DBConnection _dBConnection;

        public OrderRepository(DBConnection dBConnection)
        {
            _dBConnection = dBConnection;
        }

        public async Task<List<OrderModel>> GetOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            var orders = new List<OrderModel>();
            const string storedProcedure = "sp_GetOrders";
            try
            {
                await using var conexion = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, conexion)
                {
                    CommandType = CommandType.StoredProcedure
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

            // Filtrar los resultados si se proporciona un searchString
            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.OrderNumber.ToString()!.Contains(searchString)).ToList();
            }

            return await Task.FromResult(orders);
        }

        public async Task<int> GetTotalOrdersAsync(OrderDetailDTO orderDetailDTO, string searchString)
        {
            // Obtener todos los registros desde la base de datos
            const string storedProcedure = "sp_GetOrderByOrderNumber";
            int totalOrders = 0;
            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@OrderNumber", orderDetailDTO.OrderNumber);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                var orders = new List<OrderModel>();
                while (await reader.ReadAsync())
                {
                    orders.Add(new OrderModel
                    {
                        DocumentType = reader["DocumentType"].ToString(),
                        SupplierCode = reader["SupplierCode"].ToString(),
                        OrderNumber = (int)reader["OrderNumber"],
                        OrderDate = (DateTime)reader["OrderDate"],
                        Canceled = reader["Canceled"].ToString(),
                        IdEstatus = reader["IdEstatus"].ToString(),
                        TotalAmount = (decimal)reader["TotalAmount"],
                        DocCurOrder = reader["Currency"].ToString()!,
                        Property = reader["Property"].ToString() ?? string.Empty,
                        Invoices = (int)reader["Invoices"],
                        TotalInvoice = (decimal)reader["TotalInvoice"],
                    });
                }

                // Filtrar los resultados si se proporciona un searchString
                if (!string.IsNullOrEmpty(searchString))
                {
                    orders = orders.Where(o => o.OrderNumber.ToString()!.Contains(searchString)).ToList();
                }
                totalOrders = orders.Count;
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener el total de órdenes: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener el total de órdenes: {ex.Message}", ex);
            }
            return totalOrders;
        }

        public async Task<OrderModel> GetOrderByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            const string storedProcedure = "sp_GetOrderByOrderNumber_2";
            var order = new OrderModel();

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Parameter1", orderDetailDTO.Action);
                cmd.Parameters.AddWithValue("@Parameter2", orderDetailDTO.OrderNumber);
                cmd.Parameters.AddWithValue("@Parameter3", orderDetailDTO.SupplierCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@Parameter4", orderDetailDTO.DocumentType ?? (object)DBNull.Value);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    order = new OrderModel()
                    {
                        DocumentType = reader["DocumentType"].ToString(),
                        OrderNumber = (int)reader["OrderNumber"],
                        OrderDate = (DateTime)reader["OrderDate"],
                        Canceled = reader["Canceled"].ToString(),
                        IdEstatus = reader["IdEstatus"].ToString(),
                        TotalAmount = (decimal)reader["TotalAmount"],
                        DocCurOrder = reader["Currency"].ToString()!,
                        Property = reader["Property"].ToString() ?? string.Empty,
                        Invoices = (int)reader["Invoices"],
                        TotalInvoice = (decimal)reader["TotalInvoice"],
                    };
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener la orden por número: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener la orden por número: {ex.Message}", ex);
            }

            return order;
        }

        public async Task<List<OrderDetailModel>> GetOrderDetailsByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetailModel>();
            const string storedProcedure = "sp_GetOrderDetailsByOrderNumber";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Action", orderDetailDTO.Action);
                cmd.Parameters.AddWithValue("@OrderNumber", orderDetailDTO.OrderNumber);
                cmd.Parameters.AddWithValue("@SupplierCode", orderDetailDTO.SupplierCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentType", orderDetailDTO.DocumentType ?? (object)DBNull.Value);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new OrderDetailModel
                    {
                        DocumentTypeOrder = reader["TipoDocumento1"].ToString() ?? string.Empty,
                        OrderNumber = reader["NoDocumento1"] != DBNull.Value ? (int)reader["NoDocumento1"] : 0,
                        LineNum = reader["NoLinea1"] != DBNull.Value ? (int)reader["NoLinea1"] : 0,
                        ItemCode = reader["CodigoArticulo1"].ToString() ?? string.Empty,
                        QuantityOrder = reader["Cantidad1"] != DBNull.Value ? (decimal)reader["Cantidad1"] : 0,
                        OpenQty = reader["CantidadPendiente1"] != DBNull.Value ? (decimal)reader["CantidadPendiente1"] : 0,
                        DocCurOrder = reader["Moneda1"].ToString() ?? string.Empty,
                        Tax = reader["Impuesto1"] != DBNull.Value ? (decimal)reader["Impuesto1"] : 0,
                        PricePerItem = reader["PrecioxArticulo1"] != DBNull.Value ? (decimal)reader["PrecioxArticulo1"] : 0,
                        TotalItem = reader["LineaTotal1"] != DBNull.Value ? (decimal)reader["LineaTotal1"] : 0,
                        TotalTaxItem = reader["LineaTotalIVA1"] != DBNull.Value ? (decimal)reader["LineaTotalIVA1"] : 0,
                        TotalOrder = reader["Total1"] != DBNull.Value ? (decimal)reader["Total1"] : 0,
                        DocumentType = reader["TipoDocumento2"].ToString() ?? string.Empty,
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener los detalles de la orden: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener los detalles de la orden: {ex.Message}", ex);
            }
            return orders;
        }

        public async Task<List<OrderDetailsOfferModel>> GetOrderDetailsOfferByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<OrderDetailsOfferModel>();
            const string storedProcedure = "sp_GetOrderDetailsByOrderNumber";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Action", orderDetailDTO.Action);
                cmd.Parameters.AddWithValue("@OrderNumber", orderDetailDTO.OrderNumber);
                cmd.Parameters.AddWithValue("@SupplierCode", orderDetailDTO.SupplierCode ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@DocumentType", orderDetailDTO.DocumentType ?? (object)DBNull.Value);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new OrderDetailsOfferModel
                    {
                        DocumentType = reader["TipoDocumento1"].ToString() ?? string.Empty,
                        OrderNumber = reader["NoDocumento1"] != DBNull.Value ? (int)reader["NoDocumento1"] : 0,
                        OrderDate = reader["Fecha1"] != DBNull.Value ? (DateTime)reader["Fecha1"] : DateTime.MinValue,
                        Canceled = reader["Canceled1"].ToString() ?? string.Empty,
                        Status = reader["Status1"].ToString() ?? string.Empty,
                        TotalAmount = reader["Total1"] != DBNull.Value ? (decimal)reader["Total1"] : 0,
                        DocCurOrder = reader["Moneda1"].ToString() ?? string.Empty,
                        LineNum = reader["NoLinea1"] != DBNull.Value ? (int)reader["NoLinea1"] : 0,
                        ItemCode = reader["CodigoArticulo1"].ToString() ?? string.Empty,
                        Quantity = reader["Cantidad1"] != DBNull.Value ? (decimal)reader["Cantidad1"] : 0,
                        OpenQty = reader["CantidadPendiente1"] != DBNull.Value ? (decimal)reader["CantidadPendiente1"] : 0,
                        Price = reader["PrecioxArticulo1"] != DBNull.Value ? (decimal)reader["PrecioxArticulo1"] : 0,
                        Tax = reader["Impuesto1"] != DBNull.Value ? (decimal)reader["Impuesto1"] : 0,
                        TotalItem = reader["LineaTotal1"] != DBNull.Value ? (decimal)reader["LineaTotal1"] : 0,
                        TotalTax = reader["LineaTotalIVA1"] != DBNull.Value ? (decimal)reader["LineaTotalIVA1"] : 0,
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener los detalles de la oferta de la orden: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener los detalles de la oferta de la orden: {ex.Message}", ex);
            }
            return orders;
        }

        public async Task<List<DetailsGoodsReceiptModel>> GetOrderDetailsGoodsReceiptByOrderNumberAsync(OrderDetailDTO orderDetailDTO)
        {
            var orders = new List<DetailsGoodsReceiptModel>();
            const string storedProcedure = "sp_GetOrderDetailsByOrderNumber";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Action", orderDetailDTO.Action);
                cmd.Parameters.AddWithValue("@OrderNumber", orderDetailDTO.OrderNumber);
                cmd.Parameters.AddWithValue("@SupplierCode", orderDetailDTO.SupplierCode);
                cmd.Parameters.AddWithValue("@DocumentType", orderDetailDTO.DocumentType);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new DetailsGoodsReceiptModel
                    {
                        DocNum = reader["NoDocumento2"] != DBNull.Value ? (int)reader["NoDocumento2"] : 0,
                        InvoiceSupplier = reader["FacturaProveedor2"].ToString() ?? string.Empty,
                        ItemCode = reader["CodigoArticulo1"].ToString() ?? string.Empty,
                        OpenQty = reader["CantidadPendiente1"] != DBNull.Value ? (decimal)reader["CantidadPendiente1"] : 0,
                        DocStatus = reader["Status2"].ToString() ?? string.Empty,
                        DocCur = reader["Moneda2"].ToString() ?? string.Empty,
                        Quantity = reader["Cantidad2"] != DBNull.Value ? (decimal)reader["Cantidad2"] : 0,
                        Total = reader["Total2"] != DBNull.Value ? (decimal)reader["Total2"] : 0,
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener los detalles de la entrada de mercancia: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener los detalles de la entrada de mercancia: {ex.Message}", ex);
            }
            return orders;
        }

        public async Task<List<OrderInvoicesDTO>> GetOrderInvoicesByOrderNumberAsync(int orderNumber)
        {
            var invoices = new List<OrderInvoicesDTO>();
            const string storedProcedure = "sp_GetOrderInvoicesByOrderNumber";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    invoices.Add(new OrderInvoicesDTO
                    {
                        Invoices = reader["Invoices"] != DBNull.Value ? reader["Invoices"].ToString() : string.Empty,
                        TotalInvoice = reader["TotalInvoices"] != DBNull.Value ? (decimal)reader["TotalInvoices"] : 0,
                    });
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al obtener la informacion de las facturas: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al obtener la informacion de las facturas: {ex.Message}", ex);
            }
            return invoices;
        }
    }
}