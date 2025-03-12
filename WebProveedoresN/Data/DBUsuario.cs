using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
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
            int retryCount = 0;
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

        public static List<string> ObtenerRoles()
        {
            var roles = new List<string>();
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ListarRoles";
                using var cmd = new SqlCommand(storedProcedure, conexion)
                {
                    CommandType = CommandType.StoredProcedure
                };
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    roles.Add(dr["Nombre"].ToString());
                }
            }
            catch (Exception)
            {
                throw;
            }
            return roles;
        }

        public static List<UsuarioModel> ListarUsuariosConRoles()
        {
            var usuarios = new List<UsuarioModel>();
            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    conexion.Open();
                    var storedProcedure = "sp_ListarUsuariosConRoles";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        var usuario = new UsuarioModel
                        {
                            Empresa = dr["Empresa"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Clave = dr["Clave"].ToString(),
                            Token = dr["Token"].ToString(),
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdStatus = Convert.ToInt32(dr["IdStatus"]),
                            Roles = new List<string>()
                        };
                        if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                        {
                            usuario.Roles.Add(dr["Rol"].ToString());
                        }
                        usuarios.Add(usuario);
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return usuarios;
        }

        public static UsuarioModel ObtenerUsuario(string token)
        {
            var usuario = new UsuarioModel();
            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    conexion.Open();
                    var storedProcedure = "sp_ObtenerUsuarioPorToken";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        usuario = new UsuarioModel
                        {
                            IdUsuario = Convert.ToInt32(dr["IdUsuario"]),
                            Empresa = dr["Empresa"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Clave = dr["Clave"].ToString(),
                            Token = dr["Token"].ToString(),
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdStatus = Convert.ToInt32(dr["IdStatus"]),
                            Roles = []
                        };
                        if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                        {
                            usuario.Roles.Add(dr["Rol"].ToString());
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return usuario;
        }

        public static UsuarioModel ValidarUsuario(string correo, string clave)
        {
            var usuario = new UsuarioModel();
            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    conexion.Open();
                    var storedProcedure = "sp_ValidateUser";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.Parameters.AddWithValue("@clave", clave);
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

        public static string GuardarUsuarioConRoles(UsuarioModel usuario)
        {
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var transaction = conexion.BeginTransaction();
                var storedProcedure = "sp_GuardarUsuarioConRoles";
                try
                {
                    using var cmd = new SqlCommand(storedProcedure, conexion, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@Empresa", usuario.Empresa);
                    cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
                    cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                    cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                    cmd.Parameters.AddWithValue("@Token", usuario.Token);
                    cmd.Parameters.AddWithValue(@"IdStatus", usuario.IdStatus);
                    var idUsuario = Convert.ToInt32(cmd.ExecuteScalar());

                    foreach (var rol in usuario.Roles)
                    {
                        storedProcedure = "sp_GuardarUsuarioRol";
                        cmd.CommandText = storedProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmd.Parameters.AddWithValue("@RolNombre", rol);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return "Usuario guardado exitosamente.";
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    var error = ex.Message;
                    return $"Error al guardar los datos en SQL Server - {error}";
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                    return $"Error inesperado al guardar los datos en SQL Server - {error}";
                }
            }
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

        public static string EditarUsuarioConRoles(UsuarioModel model)
        {
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var transaction = conexion.BeginTransaction();
                var storedProcedure = "sp_EditarUsuarioConRoles";
                try
                {
                    // Guardar los datos del usuario
                    using var cmd = new SqlCommand(storedProcedure, conexion, transaction);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Nombre", model.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", model.Correo);
                    cmd.Parameters.AddWithValue("@Token", model.Token);
                    cmd.Parameters.AddWithValue(@"IdStatus", model.IdStatus);
                    cmd.ExecuteNonQuery();
                    // Eliminar los roles del usuario
                    storedProcedure = "sp_EliminarRolesUsuario";
                    cmd.CommandText = storedProcedure;
                    cmd.Parameters.Clear();
                    cmd.Parameters.AddWithValue("@IdUsuario", model.IdUsuario);
                    cmd.ExecuteNonQuery();
                    // Guardar los roles del usuario
                    foreach (var rol in model.Roles)
                    {
                        storedProcedure = "sp_GuardarUsuarioRol";
                        cmd.CommandText = storedProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@IdUsuario", model.IdUsuario);
                        cmd.Parameters.AddWithValue("@RolNombre", rol);
                        cmd.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    return "Usuario guardado exitosamente.";
                }
                catch (SqlException ex)
                {
                    transaction.Rollback();
                    var error = ex.Message;
                    return $"Error al guardar los datos en SQL Server - {error}";
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                    return $"Error inesperado al guardar los datos en SQL Server - {error}";
                }
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
    }
}
