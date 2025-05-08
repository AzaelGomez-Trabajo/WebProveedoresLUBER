using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBFiles
    {
        private readonly DBConnection _dBConnection;

        public DBFiles(DBConnection dBConnection)
        {
            _dBConnection = dBConnection;
        }

        public async Task SaveFileToDatabaseAsync(FileModel archivo)
        {
            const string storedProcedure = "sp_SaveFileToDatabase";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                };

                cmd.Parameters.AddWithValue("@Name", archivo.Name);
                cmd.Parameters.AddWithValue("@Route", archivo.Route);
                cmd.Parameters.AddWithValue("@DateTime", archivo.DateTime);
                cmd.Parameters.AddWithValue("@OrderNumber", archivo.OrderNumber);
                cmd.Parameters.AddWithValue("@Extension", archivo.Extension);
                cmd.Parameters.AddWithValue("@Converted", archivo.Converted);

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al guardar el archivo: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al guardar el archivo: {ex.Message}", ex);
            }
        }

        public async Task<string> SaveXmlDataInDatabaseAsync(List<LecturaXmlModel> archivos, string orderNumber, string supplierName, string idUsuario, string ipUsuario)
        {
            var isValid = string.Empty;
            foreach (var model in archivos)
            {
                isValid = await SearchPurchaseOrderAndInvoiceAsync(orderNumber, model.Total, supplierName);
                if (isValid != "true")
                {
                    return isValid == "false"
                        ? "La factura supera el monto faltante de la Orden de Compra "
                        : "No se encontró la Orden de Compra";
                }
            }
            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await connection.OpenAsync();
                foreach (var archivo in archivos)
                {
                    // Insertar datos en la tabla ArchivosXml
                    const string queryArchivo = @"
                        INSERT INTO ArchivosXml 
                        (SupplierCode, Folio, Serie, Version, EmisorNombre, EmisorRfc, ReceptorRfc, SubTotal, Total, UUID, Sello) 
                        OUTPUT INSERTED.Id 
                        VALUES 
                        (@SupplierCode, @Folio, @Serie, @Version, @EmisorNombre, @EmisorRfc, @ReceptorRfc, @SubTotal, @Total, @UUID, @Sello)";
                    int archivoId;
                    await using (var cmd = new SqlCommand(queryArchivo, connection))
                    {
                        cmd.Parameters.AddWithValue("@SupplierCode", archivo.SupplierCode);
                        cmd.Parameters.AddWithValue("@Folio", archivo.Folio);
                        cmd.Parameters.AddWithValue("@Serie", archivo.Serie);
                        cmd.Parameters.AddWithValue("@Version", archivo.Version);
                        cmd.Parameters.AddWithValue("@EmisorNombre", archivo.EmisorNombre);
                        cmd.Parameters.AddWithValue("@EmisorRfc", archivo.EmisorRfc);
                        cmd.Parameters.AddWithValue("@ReceptorRfc", archivo.ReceptorRfc);
                        cmd.Parameters.AddWithValue("@SubTotal", archivo.SubTotal);
                        cmd.Parameters.AddWithValue("@Total", archivo.Total);
                        cmd.Parameters.AddWithValue("@UUID", archivo.UUID);
                        cmd.Parameters.AddWithValue("@Sello", archivo.Sello);
                        // Updated line to handle potential null value from ExecuteScalarAsync
                        var result = await cmd.ExecuteScalarAsync();
                        if (result == null || result == DBNull.Value)
                        {
                            throw new Exception("El resultado de la consulta es nulo o no válido.");
                        }
                        archivoId = Convert.ToInt32(result);
                    }

                    // Inserta datos en la tabla OrdersFacturas
                    const string queryOrderFactura = @"
                        INSERT INTO OrdersFacturas 
                        (IdOrder, IdFactura, IdUsuario, IpUsuario)
                        OUTPUT INSERTED.IdFactura
                        VALUES
                        (
                            (SELECT Id FROM Orders WHERE OrderNumber = @OrderNumber),
                            @IdFactura,
                            @IdUsuario,
                            @IpUsuario
                        )";
                    await using (var cmd = new SqlCommand(queryOrderFactura, connection))
                    {
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                        cmd.Parameters.AddWithValue("@IdFactura", archivoId);
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmd.Parameters.AddWithValue("@IpUsuario", ipUsuario);
                        await cmd.ExecuteScalarAsync();
                    }

                    // Insertar datos en la tabla ConceptosXml
                    const string queryConcepto = @"
                        INSERT INTO ConceptosXml 
                        (ArchivoId, Cantidad, Descripcion, ValorUnitario, Importe)
                        VALUES 
                        (@ArchivoId, @Cantidad, @Descripcion, @ValorUnitario, @Importe)";
                    foreach (var concepto in archivo.Conceptos)
                    {
                        await using (var cmd = new SqlCommand(queryConcepto, connection))
                        {
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@ArchivoId", archivoId);
                            cmd.Parameters.AddWithValue("@Cantidad", concepto.Cantidad);
                            cmd.Parameters.AddWithValue("@Descripcion", concepto.Descripcion);
                            cmd.Parameters.AddWithValue("@ValorUnitario", concepto.ValorUnitario);
                            cmd.Parameters.AddWithValue("@Importe", concepto.Importe);
                            await cmd.ExecuteNonQueryAsync();
                        }
                    }
                }
                return "OK";
            }
            catch (SqlException ex)
            {
                return $"Error al guardar los datos en SQL Server {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error inesperado al guardar los datos en SQL Server {ex.Message}";
            }
        }

        private async Task<string> SearchPurchaseOrderAndInvoiceAsync(string orderNumber, decimal totalInvoice, string supplierName)
        {
            const string storedProcedure = "sp_ValidateInvoiceWithPurchaseOrder";
            await using var connection = await _dBConnection.GetConnectionAsync();
            await using var cmd = new SqlCommand(storedProcedure, connection)
            {
                CommandType = CommandType.StoredProcedure,
            };

            cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
            cmd.Parameters.AddWithValue("@SupplierName", supplierName);
            cmd.Parameters.AddWithValue("@TotalInvoice", totalInvoice);

            await connection.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            await connection.CloseAsync();

            return result?.ToString() ?? string.Empty;
        }

        public async Task<List<FileModel>> ObtenerDocumentosAsync(int orderNumber)
        {
            var documents = new List<FileModel>();
            const string query = @"
                SELECT Route, T0.Name, DateTime, T1.OrderNumber
                FROM Archivos T0
                INNER JOIN Orders T1 ON T0.OrderId = T1.Id
                WHERE T1.OrderNumber = @OrderNumber AND Extension = @Extension AND Converted = @Converted";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(query, connection);

                cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                cmd.Parameters.AddWithValue("@Extension", ".pdf");
                cmd.Parameters.AddWithValue("@Converted", 0);

                await connection.OpenAsync();
                await using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    documents.Add(new FileModel
                    {
                        Name = reader["Name"].ToString()!,
                        OrderNumber = Convert.ToInt32(reader["OrderNumber"]),
                        Extension = ".pdf",
                        Route = reader["Route"].ToString()!,
                        DateTime = reader["DateTime"].ToString()!,
                    });
                }
            }
            catch (SqlException ex)
            {
                // Manejar la excepción de SQL aquí
                throw new Exception($"Error al obtener documentos: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                // Manejar otras excepciones aquí
                throw new Exception($"Error inesperado al obtener lso documentos: {ex.Message}", ex);
            }
            return documents;
        }

        public async Task<bool> BuscarFacturaAsync(string UUID)
        {
            const string storedProcedure = "sp_InvoiceSearch";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await using var cmd = new SqlCommand(storedProcedure, connection)
                {
                    CommandType = CommandType.StoredProcedure,
                };

                cmd.Parameters.AddWithValue("@UUID", UUID);
                await connection.OpenAsync();
                var result = await cmd.ExecuteScalarAsync();
                return result != null && Convert.ToBoolean(result);
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al buscar la factura: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al buscar la factura: {ex.Message}", ex);
            }
        }
    }
}