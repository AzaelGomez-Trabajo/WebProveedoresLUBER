using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Data
{
    public class DBArchivos
    {
        //public static bool ValidateOrderNumberInDatabase(OrderDTO model)
        //{
        //    bool isValid = false;

        //    using (SqlConnection connection = DBConexion.ObtenerConexion())
        //    {
        //        string storedProcedure = "sp_ValidateOrderNumber";
        //        using (var cmd = new SqlCommand(storedProcedure, connection))
        //        {
        //            cmd.Parameters.AddWithValue("@NumeroOrden", model.OrderNumber);
        //            cmd.Parameters.AddWithValue("@Proveedor", model.NombreEmpresa);
        //            cmd.CommandType = CommandType.StoredProcedure;

        //            connection.Open();
        //            Console.WriteLine($"Ejecutando procedimiento almacenado para NumeroOrden: {model.OrderNumber}, Proveedor: {model.NombreEmpresa}");
        //            int count = (int)cmd.ExecuteScalar();
        //            Console.WriteLine($"Este es el resultado del SP: {count}");
        //            isValid = count > 0;
        //            connection.Close();
        //        }
        //    }
        //    return isValid;
        //}
        //public static bool ValidateOrderNumberInDatabase2(string orderNumber, string nombreProveedor)
        //{
        //    bool isValid = false;

        //    using (SqlConnection connection = DBConexion.ObtenerConexion())
        //    {
        //        string query = "SELECT COUNT(*) FROM OrdenesCompra WHERE NumeroOrden = @NumeroOrden AND Proveedor = @Proveedor";
        //        using (var cmd = new SqlCommand(query, connection))
        //        {
        //            cmd.Parameters.AddWithValue("@NumeroOrden", orderNumber);
        //            cmd.Parameters.AddWithValue("@Proveedor", nombreProveedor);
        //            cmd.CommandType = CommandType.Text;

        //            connection.Open();
        //            Console.WriteLine($"Ejecutando consulta para NumeroOrden: {orderNumber}, Proveedor: {nombreProveedor}");
        //            int count = (int)cmd.ExecuteScalar();
        //            isValid = count > 0;
        //            connection.Close();
        //        }
        //    }
        //    return isValid;
        //}

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
                //Insertar datos en la tabla 
            }
        }

        public static void GuardarDatosEnSqlServer(List<LecturaXmlDTO> archivos, int orderNumberId)
        {
            try
            {
                using (var connection = DBConexion.ObtenerConexion())
                {
                    connection.Open();
                    foreach (var archivo in archivos)
                    {
                        // Insertar datos en la tabla ArchivosXml
                        var queryArchivo = "INSERT INTO ArchivosXml (FolioFactura, Serie, Version, EmisorNombre, EmisorRfc, ReceptorRfc, SubTotal, Total, UUID) OUTPUT INSERTED.Id VALUES (@FolioFactura, @Serie, @Version, @EmisorNombre, @EmisorRfc, @ReceptorRfc, @SubTotal, @Total, @UUID)";
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

                        // Inserta datos en la tabla OrdersFacturas
                        var queryOrderFactura = "INSERT INTO OrdersFacturas (IdOrders, IdFactura) VALUES (@IdOrders, @IdFactura)";
                        using (var cmd = new SqlCommand(queryOrderFactura, connection))
                        {
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@IdOrders", orderNumberId);
                            cmd.Parameters.AddWithValue("@IdFactura", archivoId);
                            cmd.ExecuteNonQuery();
                        }

                    }
                    connection.Close();
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al guardar los datos en SQL Server {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al guardar los datos en SQL Server {ex.Message}");
            }
        }
    }
}