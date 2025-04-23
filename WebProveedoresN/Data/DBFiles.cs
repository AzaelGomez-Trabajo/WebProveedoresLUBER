using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBFiles
    {
        public static void SaveFileToDatabase(FileDTO archivo)
        {
            using (SqlConnection connection = DBConnectiion.GetConnection())
            {
                string query = $"INSERT INTO Archivos (Name, Route, DateTime, OrderId, OrderNumber, Extension, Converted) VALUES (@Name, @Route, @DateTime, SELECT Id FROM Orders WHERE OrderNumber = {archivo.OrderNumber}, @OrderNumber, @Extension, @Converted)";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@Name", archivo.Name);
                    cmd.Parameters.AddWithValue("@Route", archivo.Route);
                    cmd.Parameters.AddWithValue("@DateTime", archivo.DateTime);
                    cmd.Parameters.AddWithValue("@OrderNumber", archivo.OrderNumber);
                    cmd.Parameters.AddWithValue("@Extension", archivo.Extension);
                    cmd.Parameters.AddWithValue("@Converted", archivo.Converted);
                    cmd.CommandType = CommandType.Text;
                    connection.Open();
                    cmd.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }

        public static string SaveXmlDataInDatabase(List<LecturaXmlDTO> archivos, string orderNumber, string supplierName, string idUsuario, string ipUsuario)
        {
            var isValid = string.Empty;
            foreach (var model in archivos)
            {
                isValid = SearchPurchaseOrderAndInvoice(orderNumber, model.Total, supplierName);
            }
            if (isValid is "true")
            {
                try
                {
                    using (var connection = DBConnectiion.GetConnection())
                    {
                        connection.Open();
                        foreach (var archivo in archivos)
                        {
                            // Insertar datos en la tabla ArchivosXml
                            var queryArchivo = "INSERT INTO ArchivosXml (SupplierCode, Folio, Serie, Version, EmisorNombre, EmisorRfc, ReceptorRfc, SubTotal, Total, UUID, Sello) OUTPUT INSERTED.Id VALUES (@SupplierCode, @Folio, @Serie, @Version, @EmisorNombre, @EmisorRfc, @ReceptorRfc, @SubTotal, @Total, @UUID, @Sello)";
                            int archivoId;
                            using (var cmd = new SqlCommand(queryArchivo, connection))
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
                                cmd.CommandType = CommandType.Text;
                                archivoId = (int)cmd.ExecuteScalar();
                            }

                            // Inserta datos en la tabla OrdersFacturas
                            var queryOrderFactura = $"INSERT INTO OrdersFacturas (IdOrder, IdFactura, IdUsuario, IpUsuario) OUTPUT INSERTED.IdFactura VALUES (SELECT Id FROM Orders WHERE OrderNumber = {orderNumber}, @IdFactura, @IdUsuario, @IpUsuario)";
                            using (var cmd = new SqlCommand(queryOrderFactura, connection))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@IdFactura", archivoId);
                                cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                                cmd.Parameters.AddWithValue("@IpUsuario", ipUsuario);
                                archivoId = (int)cmd.ExecuteScalar();
                            }

                            // Insertar datos en la tabla ConceptosXml
                            var queryConcepto = "INSERT INTO ConceptosXml (ArchivoId, Cantidad, Descripcion, ValorUnitario, Importe) VALUES (@ArchivoId, @Cantidad, @Descripcion, @ValorUnitario, @Importe)";
                            foreach (var concepto in archivo.Conceptos)
                            {
                                using (var cmd = new SqlCommand(queryConcepto, connection))
                                {
                                    cmd.Parameters.Clear();
                                    cmd.Parameters.AddWithValue("@ArchivoId", archivoId);
                                    cmd.Parameters.AddWithValue("@Cantidad", concepto.Cantidad);
                                    cmd.Parameters.AddWithValue("@Descripcion", concepto.Descripcion);
                                    cmd.Parameters.AddWithValue("@ValorUnitario", concepto.ValorUnitario);
                                    cmd.Parameters.AddWithValue("@Importe", concepto.Importe);
                                    cmd.CommandType = CommandType.Text;
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        connection.Close();
                        return "OK";
                    }
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
            if (isValid is "false")
            {
                return "La factura supera el monto faltante de la Orden de Compra ";
            }
            return "No se encontró la Orden de Compra ";
        }

        private static string SearchPurchaseOrderAndInvoice(string orderNumber, decimal totalInvoice, string supplierName)
        {
            var isValid = string.Empty;
            string storedProcedure = "sp_ValidateInvoiceWithPurchaseOrder";
            using (var connection = DBConnectiion.GetConnection())
            {
                using (var cmd = new SqlCommand(storedProcedure, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.Parameters.AddWithValue("@SupplierName", supplierName);
                    cmd.Parameters.AddWithValue("@TotalInvoice", totalInvoice);
                    cmd.CommandType = CommandType.StoredProcedure;
                    isValid = cmd.ExecuteScalar().ToString();
                }
            }
            return isValid;
        }

        public static async Task<List<FileDTO>> ObtenerDocumentosAsync(string orderNumber)
        {
            var documents = new List<FileDTO>();

            using (var connection = DBConnectiion.GetConnection())
            {
                string query = "SELECT Route, T0.Name, DateTime, T1.OrderNumber FROM Archivos T0 INNER JOIN Orders T1 ON T0.OrderId = T1.Id WHERE T1.OrderNumber = @OrderNumber AND Extension = @Extension AND Converted = @Converted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                command.Parameters.AddWithValue("@Extension", ".pdf");
                command.Parameters.AddWithValue("@Converted", 0);

                await connection.OpenAsync();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        documents.Add(new FileDTO
                        {
                            Name = reader["Name"].ToString(),
                            Extension = ".pdf",
                            Route = reader["Route"].ToString(),
                            DateTime = reader["DateTime"].ToString(),
                        });
                    }
                }
            }
            return documents;
        }

        public static bool BuscarFactura(string UUID)
        {
            bool isValid = false;
            string storedProcedure = "sp_InvoiceSearch";
            using (var connection = DBConnectiion.GetConnection())
            {
                using (var cmd = new SqlCommand(storedProcedure, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@UUID", UUID);
                    cmd.CommandType = CommandType.StoredProcedure;
                    isValid = Convert.ToBoolean(cmd.ExecuteScalar());
                }
                connection.Close();
            }
            return isValid;
        }
    }
}