using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Data
{
    public static class DBStart
    {
        private const int MaxRetryCount = 3;

        public static bool ValidateSupplier(Supplier supplier)
        {
            using var conexion = DBConnectiion.GetConnection();
            conexion.Open();
            var storedProcedure = "sp_ValidateSupplier";
            using var cmd = new SqlCommand(storedProcedure, conexion);
            cmd.Parameters.AddWithValue("@Code", supplier.Code);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.CommandType = CommandType.StoredProcedure;
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        public static bool ValidateFirstUser(Supplier supplier)
        {
            using var conexion = DBConnectiion.GetConnection();
            conexion.Open();
            var storedProcedure = "sp_ValidateFirstUser";
            using var cmd = new SqlCommand(storedProcedure, conexion);
            cmd.Parameters.AddWithValue("@Code", supplier.Code);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.CommandType = CommandType.StoredProcedure;
            return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        public static List<Usuario> ListarUsuariosConRoles(string supplierCode)
        {
            var usuarios = new List<Usuario>();
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_ListarUsuariosConRoles";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.Parameters.AddWithValue("@SupplierCode", supplierCode);
                cmd.CommandType = CommandType.StoredProcedure;
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var usuario = new Usuario
                    {
                        Nombre = dr["Nombre"].ToString()!,
                        Correo = dr["Correo"].ToString()!,
                        Clave = dr["Clave"].ToString()!,
                        Token = dr["Token"].ToString()!,
                        Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                        Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                        IdStatus = Convert.ToInt32(dr["StatusId"]),
                        Roles = new List<string>()
                    };
                    if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                    {
                        usuario.Roles.Add(dr["Rol"].ToString()!);
                    }
                    usuarios.Add(usuario);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return usuarios;
        }

        public static Usuario GetUserByToken(string token)
        {
            var usuario = new Usuario();
            try
            {
                using (var conexion = DBConnectiion.GetConnection())
                {
                    conexion.Open();
                    var storedProcedure = "sp_GetUserByToken";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = cmd.ExecuteReader();
                    while (dr.Read())
                    {
                        usuario = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(dr["Id"]),
                            Nombre = dr["Nombre"].ToString()!,
                            SupplierCode = dr["SupplierCode"].ToString()!,
                            SupplierName = dr["Empresa"].ToString()!,
                            Correo = dr["Correo"].ToString()!,
                            Clave = dr["Clave"].ToString()!,
                            Token = dr["Token"].ToString()!,
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdStatus = Convert.ToInt32(dr["StatusId"]),
                            Roles = []
                        };
                        if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                        {
                            usuario.Roles.Add(dr["Rol"].ToString()!);
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

        public static Usuario ValidateUser(string correo, string clave)
        {
            var usuario = new Usuario();
            try
            {
                using (var conexion = DBConnectiion.GetConnection())
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
                        usuario = new Usuario
                        {
                            IdUsuario = Convert.ToInt32(dr["Id"]),
                            SupplierCode = dr["Code"].ToString()!,
                            SupplierName = dr["Name"].ToString()!,
                            Nombre = dr["Nombre"].ToString()!,
                            Correo = correo,
                            Clave = clave,
                            Token = dr["Token"].ToString()!,
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdStatus = Convert.ToInt32(dr["StatusId"]),
                            Roles = []
                        };
                        if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                        {
                            usuario.Roles.Add(dr["Rol"].ToString()!);
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

        public static string SaveUserWithRoles(Usuario usuario)
        {
            using var conexion = DBConnectiion.GetConnection();
            conexion.Open();
            var transaction = conexion.BeginTransaction();
            var storedProcedure = "sp_SaveUserWithRoles";
            try
            {
                using var cmd = new SqlCommand(storedProcedure, conexion, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@SupplierCode", usuario.SupplierCode);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
                cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.Parameters.AddWithValue("@StatusId", usuario.IdStatus);
                var idUsuario = Convert.ToInt32(cmd.ExecuteScalar());

                //foreach (var rol in usuario.Roles)
                //{
                storedProcedure = "sp_SaveUserRole";
                cmd.CommandText = storedProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UsuarioId", idUsuario);
                cmd.Parameters.AddWithValue("@RoleId", 1);
                cmd.ExecuteNonQuery();
                //}
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

        public static string SaveGuestWithRoles(Usuario usuario)
        {
            using var conexion = DBConnectiion.GetConnection();
            conexion.Open();
            var transaction = conexion.BeginTransaction();
            var storedProcedure = "sp_SaveUserWithRoles";
            try
            {
                using var cmd = new SqlCommand(storedProcedure, conexion, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@SupplierCode", usuario.SupplierCode);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@Clave", usuario.Clave);
                cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.Parameters.AddWithValue("@StatusId", usuario.IdStatus);
                var idUsuario = Convert.ToInt32(cmd.ExecuteScalar());

                //foreach (var rol in usuario.Roles)
                //{
                storedProcedure = "sp_SaveUserRole";
                cmd.CommandText = storedProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UsuarioId", idUsuario);
                cmd.Parameters.AddWithValue("@RoleId", 2);
                cmd.ExecuteNonQuery();
                //}
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

        public static string EditarUsuarioConRoles(Usuario model)
        {
            using (var conexion = DBConnectiion.GetConnection())
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
                    cmd.Parameters.AddWithValue(@"StatusId", model.IdStatus);
                    var idUsuario = Convert.ToInt32(cmd.ExecuteScalar());
                    foreach (var rol in model.Roles)
                    {
                        storedProcedure = "sp_GuardarUsuarioRol";
                        cmd.CommandText = storedProcedure;
                        cmd.Parameters.Clear();
                        cmd.Parameters.AddWithValue("@UsuarioId", idUsuario);
                        cmd.Parameters.AddWithValue("@RoleId", rol);
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

        public static string Editar(Usuario usuario)
        {
            try
            {
                using var conexion = DBConnectiion.GetConnection();
                conexion.Open();
                var storedProcedure = "sp_Editar";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.Nombre);
                cmd.Parameters.AddWithValue("@Correo", usuario.Correo);
                cmd.Parameters.AddWithValue("@StatusId", usuario.IdStatus);
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

        public static string ResetPassword(UpdateDTO updateDTO)
        {
            try
            {
                using (var oconexion = DBConnectiion.GetConnection())
                {
                    oconexion.Open();
                    var storedProcedure = "sp_ResetPassword";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Restablecer", updateDTO.Restablecer);
                    cmd.Parameters.AddWithValue("@Clave", updateDTO.Password);
                    cmd.Parameters.AddWithValue("@Token", updateDTO.Token);
                    cmd.ExecuteNonQuery();
                    return "Usuario guardado exitosamente.";
                }
            }
            catch (SqlException ex)
            {
                return $"Error al guardar los datos en SQL Server - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error inesperado al guardar los datos en SQL Server - {ex.Message}";
            }
        }

        public static string UpdatePassword(UpdateDTO updateDTO)
        {
            try
            {
                using (var oconexion = DBConnectiion.GetConnection())
                {
                    oconexion.Open();
                    var storedProcedure = "sp_UpdatePassword";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Restablecer", updateDTO.Restablecer);
                    cmd.Parameters.AddWithValue("@Clave", updateDTO.Password);
                    cmd.Parameters.AddWithValue("@Token", updateDTO.Token);
                    cmd.ExecuteNonQuery();
                    return "Usuario guardado exitosamente.";
                }
            }
            catch (SqlException ex)
            {
                return $"Error al guardar los datos en SQL Server - {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"Error inesperado al guardar los datos en SQL Server - {ex.Message}";
            }
        }


        public static string ConfirmEmail(string token)
        {
            try
            {
                using (var oconexion = DBConnectiion.GetConnection())
                {
                    oconexion.Open();
                    var storedProcedure = "sp_ConfirmEmail";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.ExecuteNonQuery();
                    return "Se confirmo exitosamente.";
                }
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

        public static Usuario GetUser(EmailDTO emailDTO)
        {
            Usuario usuario = null!;
            try
            {
                using (SqlConnection oconexion = DBConnectiion.GetConnection())
                {
                    string query = "SELECT Nombre, Clave, Restablecer, Confirmado, Token FROM Usuario";
                    query += " where Correo = @correo";

                    var cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@correo", emailDTO.Email);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            usuario = new Usuario()
                            {
                                Nombre = dr["Nombre"].ToString()!,
                                Clave = dr["Clave"].ToString()!,
                                Restablecer = (bool)dr["Restablecer"],
                                Confirmado = (bool)dr["Confirmado"],
                                Token = dr["Token"].ToString()!
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return usuario;
        }

        public static bool Registrar(Usuario usuario)
        {
            bool respuesta = false;
            try
            {
                using (var conexion = DBConnectiion.GetConnection())
                {
                    string query = "INSERT INTO Usuario (Nombre, SupplierCode, Correo, Clave, Restablecer, Confirmado ,Token)";
                    query += " values(@nombre, @SupplierCode, @correo, @clave, @restablecer, @confirmado, @token)";

                    var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@SupplierCode", usuario.SupplierCode);
                    cmd.Parameters.AddWithValue("@correo", usuario.Correo);
                    cmd.Parameters.AddWithValue("@clave", usuario.Clave);
                    cmd.Parameters.AddWithValue("@restablecer", usuario.Restablecer);
                    cmd.Parameters.AddWithValue("@confirmado", usuario.Confirmado);
                    cmd.Parameters.AddWithValue("@token", usuario.Token);
                    cmd.CommandType = CommandType.Text;

                    conexion.Open();

                    int filasAfectadas = cmd.ExecuteNonQuery();
                    if (filasAfectadas > 0)
                    {
                        respuesta = true;

                        // Enviar correo de confirmación
                        var correo = new EmalDTO
                        {
                            Para = usuario.Correo,
                            Asunto = "Confirmación de registro",
                            Contenido = $"Hola {usuario.Nombre},<br><br>Gracias por registrarte. Por favor, confirma tu correo electrónico haciendo clic en el siguiente enlace:<br><a href='https://tu-sitio.com/confirmar?token={usuario.Token}'>Confirmar correo</a><br><br>Saludos,<br>El equipo de LUBER Lubricantes"
                        };

                        CorreoServicio.EnviarCorreo(correo, usuario.Nombre);
                    }
                }

                return respuesta;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}