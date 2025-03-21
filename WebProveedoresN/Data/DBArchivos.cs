using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public class DBArchivos
    {
        public static void SaveFileToDatabase(ArchivoDTO archivo)
        {
            using (SqlConnection connection = DBConexion.ObtenerConexion())
            {
                string query = "INSERT INTO Archivos (Name, Route, DateTime, OrderNumber, Extension, Converted) VALUES (@Name, @Route, @DateTime, @OrderNumber, @Extension, @Converted)";
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

        public static string GuardarDatosEnSqlServer(List<LecturaXmlDTO> archivos, string orderNumber)
        {

            var isValid = false;
            foreach (var model in archivos)
            {
                isValid = BuscarOrdenCompraYFactura(orderNumber, model.Total);
            }
            if (isValid)
            {
                try
                {
                    using (var connection = DBConexion.ObtenerConexion())
                    {
                        connection.Open();
                        foreach (var archivo in archivos)
                        {
                            // Insertar datos en la tabla ArchivosXml
                            var queryArchivo = "INSERT INTO ArchivosXml (SupplierId, FolioFactura, Serie, Version, EmisorNombre, EmisorRfc, ReceptorRfc, SubTotal, Total, UUID) OUTPUT INSERTED.IdFactura VALUES (@SupplierId, @FolioFactura, @Serie, @Version, @EmisorNombre, @EmisorRfc, @ReceptorRfc, @SubTotal, @Total, @UUID)";
                            int archivoId;
                            using (var cmd = new SqlCommand(queryArchivo, connection))
                            {
                                cmd.Parameters.AddWithValue("@SupplierId", archivo.SupplierId);
                                cmd.Parameters.AddWithValue("@FolioFactura", archivo.FolioFactura);
                                cmd.Parameters.AddWithValue("@Serie", archivo.Serie);
                                cmd.Parameters.AddWithValue("@Version", archivo.Version);
                                cmd.Parameters.AddWithValue("@EmisorNombre", archivo.EmisorNombre);
                                cmd.Parameters.AddWithValue("@EmisorRfc", archivo.EmisorRfc);
                                cmd.Parameters.AddWithValue("@ReceptorRfc", archivo.ReceptorRfc);
                                cmd.Parameters.AddWithValue("@SubTotal", archivo.SubTotal);
                                cmd.Parameters.AddWithValue("@Total", archivo.Total);
                                cmd.Parameters.AddWithValue("@UUID", archivo.UUID);
                                cmd.CommandType = CommandType.Text;
                                archivoId = (int)cmd.ExecuteScalar();
                            }

                            // Inserta datos en la tabla OrdersFacturas
                            var queryOrderFactura = "INSERT INTO OrdersFacturas (OrderNumber, IdFactura) OUTPUT INSERTED.IdFactura VALUES (@OrderNumber, @IdFactura)";
                            using (var cmd = new SqlCommand(queryOrderFactura, connection))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.Clear();
                                cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                                cmd.Parameters.AddWithValue("@IdFactura", archivoId);
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
            return "La factura supera el monto faltante de la Orden de Compra";
        }

        private static bool BuscarOrdenCompraYFactura(string orderNumber, decimal totalInvoice)
        {
            bool isValid = false;
            string storedProcedure = "sp_ValidarFacturaConOrdenCompra";
            using (var connection = DBConexion.ObtenerConexion())
            {
                using (var cmd = new SqlCommand(storedProcedure, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@OrderNumber", orderNumber);
                    cmd.Parameters.AddWithValue("@TotalInvoice", totalInvoice);
                    cmd.CommandType = CommandType.StoredProcedure;
                    isValid = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            return isValid;
        }

        public async static Task<List<ArchivoDTO>> ObtenerDocumentosAsync(string orderNumber, int converted)
        {
            var documents = new List<ArchivoDTO>();

            using (var connection = DBConexion.ObtenerConexion())
            {
                string query = "SELECT Route, Name, DateTime FROM Archivos WHERE OrderNumber = @OrderNumber AND Extension = @Extension AND Converted = @Converted";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@OrderNumber", orderNumber);
                command.Parameters.AddWithValue("@Extension", ".pdf");
                command.Parameters.AddWithValue("@Converted", converted);

                await connection.OpenAsync();
                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        documents.Add(new ArchivoDTO
                        {
                            Name = reader["Name"].ToString(),
                            Extension = ".pdf",
                            Route = reader["Route"].ToString(),
                            DateTime = reader["DateTime"].ToString(),
                            OrderNumber = orderNumber,
                        });
                    }
                }
            }
            return documents;
        }



    }
}