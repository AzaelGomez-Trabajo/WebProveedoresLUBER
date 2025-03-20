using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

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

        public static string GuardarDatosEnSqlServer(List<LecturaXmlDTO> archivos, int orderNumberId)
        {
            var isValid = false;
            foreach (var model in archivos)
            {
                isValid = BuscarOrdenCompraYFactura(orderNumberId, model.Total);
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
                            var queryArchivo = "INSERT INTO ArchivosXml (FolioFactura, Serie, Version, EmisorNombre, EmisorRfc, ReceptorRfc, SubTotal, Total, UUID) OUTPUT INSERTED.IdFactura VALUES (@FolioFactura, @Serie, @Version, @EmisorNombre, @EmisorRfc, @ReceptorRfc, @SubTotal, @Total, @UUID)";
                            int archivoId;
                            using (var cmd = new SqlCommand(queryArchivo, connection))
                            {
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
                            var queryOrderFactura = "INSERT INTO OrdersFacturas (IdOrders, IdFactura) VALUES (@IdOrders, @IdFactura)";
                            using (var cmd = new SqlCommand(queryOrderFactura, connection))
                            {
                                cmd.CommandType = CommandType.Text;
                                cmd.Parameters.AddWithValue("@IdOrders", orderNumberId);
                                cmd.Parameters.AddWithValue("@IdFactura", archivoId);
                                cmd.ExecuteNonQuery();
                            }

                            // Insertar datos en la tabla ConceptosXml
                            var queryConcepto = "INSERT INTO ConceptosXml (ArchivoId, Cantidad, Descripcion, ValorUnitario, Importe) VALUES (@ArchivoId, @Cantidad, @Descripcion, @ValorUnitario, @Importe)";
                            foreach (var concepto in archivo.Conceptos)
                            {
                                using (var cmd = new SqlCommand(queryConcepto, connection))
                                {
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

        private static bool BuscarOrdenCompraYFactura(int orderNumberId, decimal totalInvoice)
        {
            bool isValid = false;
            string storedProcedure = "sp_ValidarFacturaConOrdenCompra";
            using (var connection = DBConexion.ObtenerConexion())
            {
                using (var cmd = new SqlCommand(storedProcedure, connection))
                {
                    connection.Open();
                    cmd.Parameters.AddWithValue("@IdOrder", orderNumberId);
                    cmd.Parameters.AddWithValue("@TotalInvoice", totalInvoice);
                    cmd.CommandType = CommandType.StoredProcedure;
                    isValid = Convert.ToBoolean(cmd.ExecuteScalar());
                }
            }
            return isValid;
        }
    }
}