using iTextSharp.text.pdf;
using iTextSharp.text;
using Microsoft.Data.SqlClient;
using SAT.Services.ConsultaCFDIService;
using SW.Services.Status;
using System.Data;
using System.Xml.Linq;
using System.Xml;
using WebProveedoresN.Data;
using WebProveedoresN.Models;
using WebProveedoresN.Repositories.Interfaces;
using System;

namespace WebProveedoresN.Repositories.Implementations
{
    public class FilesRepository : IFilesRepository
    {
        private readonly DBConnection _dBConnection;

        public FilesRepository(DBConnection dBConnection)
        {
            _dBConnection = dBConnection;
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

        public string ConvertXmlToPdf(string xmlContent, string pdfFilePath)
        {
            if (string.IsNullOrWhiteSpace(xmlContent)) throw new ArgumentException("El contenido XML no puede estar vacío.", nameof(xmlContent));
            if (string.IsNullOrWhiteSpace(pdfFilePath)) throw new ArgumentException("La ruta del archivo PDF no puede estar vacía.", nameof(pdfFilePath));

            try
            {
                using var fs = new FileStream(pdfFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using var document = new Document();
                using var writer = PdfWriter.GetInstance(document, fs);

                document.Open();

                // Agregar contenido del XML al PDF
                var parseXml = XElement.Parse(xmlContent);
                document.Add(new Paragraph(parseXml.ToString()));

                document.Close();
                writer.Close();

                return document.ToString()!;
            }
            catch (XmlException ex)
            {
                throw new Exception($"Error al analizar el contenido XML: {ex.Message}", ex);
            }
            catch (IOException ex)
            {
                throw new Exception($"Error al escribir el archivo PDF: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al convertir XML a PDF: {ex.Message}", ex);
            }
        }

        public List<LecturaXmlModel> GetDataFromXml(string xmlContent)
        {
            var archivos = new List<LecturaXmlModel>();
            try
            {
                var xmlDoc = XDocument.Parse(xmlContent);
                foreach (var element in xmlDoc.Descendants("{http://www.sat.gob.mx/cfd/4}Comprobante"))
                {
                    var archivo = new LecturaXmlModel
                    {
                        // Comprobante
                        Folio = element.Attribute("Folio")?.Value ?? "0",
                        Version = double.TryParse(element.Attribute("Version")?.Value, out var version) ? version : 0,
                        Serie = element.Attribute("Serie")?.Value ?? string.Empty,
                        Sello = element.Attribute("Sello")?.Value ?? string.Empty,
                        // Provedor
                        EmisorNombre = element.Element("{http://www.sat.gob.mx/cfd/4}Emisor")?.Attribute("Nombre")?.Value ?? string.Empty,
                        EmisorRfc = element.Element("{http://www.sat.gob.mx/cfd/4}Emisor")?.Attribute("Rfc")?.Value ?? string.Empty,
                        // RFC a comparar
                        ReceptorRfc = element.Element("{http://www.sat.gob.mx/cfd/4}Receptor")?.Attribute("Rfc")?.Value ?? string.Empty,
                        // Importes
                        SubTotal = decimal.TryParse(element.Attribute("SubTotal")?.Value, out var subTotal) ? subTotal : 0,
                        Total = decimal.TryParse(element.Attribute("Total")?.Value, out var total) ? total : 0,
                        UUID = element.Element("{http://www.sat.gob.mx/cfd/4}Complemento")?.Element("{http://www.sat.gob.mx/TimbreFiscalDigital}TimbreFiscalDigital")?.Attribute("UUID")?.Value ?? string.Empty,
                        Conceptos = []
                    };

                    // Conceptos
                    foreach (var conceptoElement in element.Descendants("{http://www.sat.gob.mx/cfd/4}Concepto"))
                    {
                        var concepto = new ConceptModel
                        {
                            Cantidad = decimal.TryParse(conceptoElement.Attribute("Cantidad")?.Value, out var cantidad) ? cantidad : 0,
                            Descripcion = conceptoElement.Attribute("Descripcion")?.Value ?? string.Empty,
                            ValorUnitario = decimal.TryParse(conceptoElement.Attribute("ValorUnitario")?.Value, out var valorUnitario) ? valorUnitario : 0,
                            Importe = decimal.TryParse(conceptoElement.Attribute("Importe")?.Value, out var importe) ? importe : 0
                        };
                        archivo.Conceptos.Add(concepto);
                    }

                    archivos.Add(archivo);
                }
            }
            catch (XmlException ex)
            {
                Console.WriteLine($"Error al analizar el contenido XML: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al procesar el XML: {ex.Message}");
            }
            return archivos;
        }

        public async Task<List<FileModel>> GetDocumentsAsync(int orderNumber)
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

        public async Task<List<CFDIModel>> GetCFDIStatusAsync(CFDIStatusModel cFDIStatusModel)
        {
            if (cFDIStatusModel == null) throw new ArgumentException("Todos los parámetros son obligatorios.");
            try
            {
                var status = new Status("https://consultaqr.facturaelectronica.sat.gob.mx/ConsultaCFDIService.svc");
                var response = await Task.Run(() => status.GetStatusCFDI(cFDIStatusModel.EmisorRFC, cFDIStatusModel.ReceptorRFC, cFDIStatusModel.Total, cFDIStatusModel.UUID, cFDIStatusModel.Sello));

                var cfdiStatus = new CFDIModel
                {
                    // Verificar el estado del CFDI
                    Estado = response.Estado,
                    Codigo_Estatus = response.CodigoEstatus,
                    Es_Cancelable = response.EsCancelable,
                    Cancelacion_Estatus = response.EstatusCancelacion,
                };
                return [cfdiStatus];
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al consultar el estado del CFDI: {ex.Message}", ex);
            }
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

        public async Task SaveFilesToDatabaseAsync(List<FileModel> archivos)
        {
            if (archivos == null || !archivos.Any()) throw new ArgumentException("La lista de archivos no puede estar vacía.", nameof(archivos));
            const string storedProcedure = "sp_SaveFileToDatabase";

            try
            {
                await using var connection = await _dBConnection.GetConnectionAsync();
                await connection.OpenAsync();

                foreach (var archivo in archivos)
                {
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

                    await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                throw new Exception($"Error al guardar el archivo en la base de datos: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado al guardar los archivos: {ex.Message}", ex);
            }
        }

        public async Task<string> SaveXmlDataInDatabaseAsync(List<LecturaXmlModel> archivos, int orderNumber, string supplierName, string idUsuario, string ipUsuario)
        {
            string isValid;
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
                        await using var cmd = new SqlCommand(queryConcepto, connection);
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@ArchivoId", archivoId);
                        cmd.Parameters.AddWithValue("@Cantidad", concepto.Cantidad);
                        cmd.Parameters.AddWithValue("@Descripcion", concepto.Descripcion);
                        cmd.Parameters.AddWithValue("@ValorUnitario", concepto.ValorUnitario);
                        cmd.Parameters.AddWithValue("@Importe", concepto.Importe);
                        await cmd.ExecuteNonQueryAsync();
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

        public async Task<bool> SearchInvoiceAsync(string UUID)
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
                if(result == null || result == DBNull.Value)
                {
                    return false;
                }
                return Convert.ToInt32(result) == 1;
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

        public async Task<string> SearchPurchaseOrderAndInvoiceAsync(int orderNumber, decimal totalInvoice, string supplierName)
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

    }
}