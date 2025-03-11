using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using NuGet.Common;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;

namespace WebProveedoresN.Data
{
    public static class DBUsuario
    {
        private const int MaxRetryCount = 3;
        public static List<UsuarioModel> Listar()
        {
            var lista = new List<UsuarioModel>();
            int retryCount =0;
            while (retryCount < MaxRetryCount)
            {
                try
                {
                    using var conexion = DBConexion.ObtenerConexion();
                    conexion.Open();
                    var storedProcedure = "sp_Listar";
                    var cmd = new SqlCommand(storedProcedure, conexion)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        lista.Add(new UsuarioModel()
                        {
                            Empresa = dr["Empresa"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Clave = dr["Clave"].ToString(),
                            Token = dr["Token"].ToString(),
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdAcceso = Convert.ToInt32(dr["IdAcceso"]),
                            IdStatus = Convert.ToInt32(dr["IdStatus"]),
                        });
                    }
                    break; // Sale del loop if funciona correctamente
                }
                catch (SqlException ex)
                {
                    retryCount++;
                    if (retryCount >= MaxRetryCount)
                    {
                        throw new Exception($"Error al intentar conectar a la base de datos después de varios intentos. {ex.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error inesperado al guardar los datos en SQL Server {ex.Message}");
                    retryCount++;
                }
            }
                return lista;
        }

        public static UsuarioModel Obtener(string token)
        {
            var usuario = new UsuarioModel();
            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    conexion.Open();
                    var storedProcedure = "sp_Obtener";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        usuario.Empresa = dr["Empresa"].ToString();
                        usuario.Nombre = dr["Nombre"].ToString();
                        usuario.Correo = dr["Correo"].ToString();
                        usuario.Clave = dr["Clave"].ToString();
                        usuario.Token = dr["Token"].ToString();
                        usuario.Restablecer = Convert.ToBoolean(dr["Restablecer"]);
                        usuario.Confirmado = Convert.ToBoolean(dr["Confirmado"]);
                        usuario.IdAcceso = Convert.ToInt32(dr["IdAcceso"]);
                        usuario.IdStatus = Convert.ToInt32(dr["IdStatus"]);
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return usuario;
        }

        public static string Guardar(UsuarioModel usuario)
        {
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_Guardar";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Empresa", usuario.Empresa);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
                cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.Parameters.AddWithValue(@"IdAcceso", usuario.IdAcceso);
                cmd.Parameters.AddWithValue(@"IdStatus", usuario.IdStatus);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.ExecuteNonQuery();
                return "Usuario guardado exitosamente.";
            }
            catch (SqlException ex)
            {
                var error = ex.Message;
                return $"Error al guardar los datos en SQL Server - {error}";

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return $"Error inesperado al guardar los datos en SQL Server - {error}";
            }
        }

        public static string Editar(UsuarioModel usuario)
        {
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_Editar";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@IdAcceso", usuario.IdAcceso);
                cmd.Parameters.AddWithValue("@IdStatus", usuario.IdStatus);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.ExecuteNonQuery();
                return "Usuario guardado exitosamente.";
            }
            catch (SqlException ex)
            {
                var error = ex.Message;
                return $"Error al guardar los datos en SQL Server - {error}";

            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return $"Error inesperado al guardar los datos en SQL Server - {error}";
            }
        }

        public static bool ChangeStatus(string Token, int Status)
        {
            bool rpta;

            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ChangeStatus";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", Token);
                    cmd.Parameters.AddWithValue("@IdState", Status);
                    cmd.ExecuteNonQuery();
                }
                rpta = true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al guardar los datos en SQL Server {ex.Message}");
                rpta = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al guardar los datos en SQL Server {ex.Message}");
                rpta = false;
            }
            return rpta;
        }

        public static bool ChangeAccess(string Token, int Access)
        {
            bool rpta;

            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ChangeAccess";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Token", Token);
                    cmd.Parameters.AddWithValue("@IdAcceso", Access);
                    cmd.ExecuteNonQuery();
                }
                rpta = true;
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error al guardar los datos en SQL Server {ex.Message}");
                rpta = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al guardar los datos en SQL Server {ex.Message}");
                rpta = false;
            }
            return rpta;
        }


    }
}
