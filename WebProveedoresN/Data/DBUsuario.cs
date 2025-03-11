using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
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
                    using (var conexion = DBConexion.ObtenerConexion())
                    {
                        conexion.Open();
                        var storedProcedure = "sp_Listar";
                        var cmd = new SqlCommand(storedProcedure, conexion);
                        cmd.CommandType = CommandType.StoredProcedure;
                        using var dr = cmd.ExecuteReader();
                        while (dr.Read())
                        {
                            lista.Add(new UsuarioModel()
                            {
                                Id = Convert.ToInt32(dr["Id"]),
                                Empresa = dr["Empresa"].ToString(),
                                Nombre = dr["Nombre"].ToString(),
                                Correo = dr["Correo"].ToString(),
                                Clave = dr["Clave"].ToString(),
                                Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                                Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            });
                        }
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

        public static UsuarioModel Obtener(int id)
        {
            var usuario = new UsuarioModel();
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var storedProcedure = "sp_Obtener";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.CommandType = CommandType.StoredProcedure;
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    usuario.Id = Convert.ToInt32(dr["Id"]);
                    usuario.Empresa = dr["Empresa"].ToString();
                    usuario.Nombre = dr["Nombre"].ToString();
                    usuario.Correo = dr["Correo"].ToString();
                    usuario.Clave = dr["Clave"].ToString();
                    usuario.Restablecer = Convert.ToBoolean(dr["Restablecer"]);
                    usuario.Confirmado = Convert.ToBoolean(dr["Confirmado"]);
                    usuario.Token = dr["Token"].ToString();
                    usuario.IdAcceso = Convert.ToInt32(dr["IdAcceso"]);
                    usuario.IdStatus = Convert.ToInt32(dr["IdState"]);
                }
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

        public static bool Editar(UsuarioModel usuario)
        {
            bool rpta;

            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_Editar";
                using (var cmd = new SqlCommand(storedProcedure, conexion))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Empresa", usuario.Empresa);
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
                    cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                    cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                    cmd.Parameters.AddWithValue("@Token", usuario.Token);
                    cmd.Parameters.AddWithValue("@IdAcceso", usuario.IdAcceso);
                    cmd.Parameters.AddWithValue("@IdStatus", usuario.IdStatus);
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
