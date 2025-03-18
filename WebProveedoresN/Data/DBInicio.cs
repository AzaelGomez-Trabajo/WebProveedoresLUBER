using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Conexion;
using WebProveedoresN.Models;
using WebProveedoresN.Services;

namespace WebProveedoresN.Data
{
    public static class DBInicio
    {
        private const int MaxRetryCount = 3;

        public static string ValidarPrimerUsuario(EmpresaDTO empresa)
        {
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ValidarPrimerUsuario";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.Parameters.AddWithValue("@Empresa", empresa.Empresa);
                cmd.Parameters.AddWithValue("@Clave", empresa.Clave);
                cmd.CommandType = CommandType.StoredProcedure;
                var respuesta = cmd.ExecuteScalar().ToString();
                return respuesta;
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

        public static List<UsuarioDTO> Listar()
        {
            var lista = new List<UsuarioDTO>();
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
                        lista.Add(new UsuarioDTO()
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
                    break; // Sale del loop si funciona correctamente
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

        public static List<UsuarioDTO> ListarUsuariosConRoles()
        {
            var usuarios = new List<UsuarioDTO>();
            try
            {
                using var conexion = DBConexion.ObtenerConexion();
                conexion.Open();
                var storedProcedure = "sp_ListarUsuariosConRoles";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                using var dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    var usuario = new UsuarioDTO
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
            catch (Exception)
            {
                throw;
            }
            return usuarios;
        }

        public static UsuarioDTO ObtenerUsuarioConToken(string token)
        {
            var usuario = new UsuarioDTO();
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
                        usuario = new UsuarioDTO
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

        public static UsuarioDTO ValidarUsuario(string correo, string clave)
        {
            var usuario = new UsuarioDTO();
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

        public static string GuardarUsuarioConRoles(UsuarioDTO usuario)
        {
            using var conexion = DBConexion.ObtenerConexion();
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
                // Enviar correo de confirmación
                var correo = new CorreoDTO
                {
                    Para = usuario.Correo,
                    Asunto = "Confirmación de registro",
                    Contenido = $"Hola {usuario.Nombre},<br><br>Gracias por registrarte. Por favor, confirma tu correo electrónico haciendo clic en el siguiente enlace:<br><a href='https://tu-sitio.com/confirmar?token={usuario.Token}'>Confirmar correo</a><br><br>Saludos,<br>El equipo de LUBER Lubricantes"
                };

                CorreoServicio.EnviarCorreo(correo, usuario.Nombre);

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

        public static string Guardar(UsuarioDTO usuario)
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

        public static string EditarUsuarioConRoles(UsuarioDTO model)
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

        public static string Editar(UsuarioDTO usuario)
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

        public static string RestablecerActualizar(int restablecer, string clave, string token)
        {
            try
            {
                using (var oconexion = DBConexion.ObtenerConexion())
                {
                    oconexion.Open();
                    var storedProcedure = "sp_RestablecerActualizar";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@restablecer", restablecer);
                    cmd.Parameters.AddWithValue("@clave", clave);
                    cmd.Parameters.AddWithValue("@token", token);
                    cmd.ExecuteNonQuery();
                    return "Usuario guardado exitosamente.";
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

        public static string Confirmar(string token)
        {
            try
            {
                using (var oconexion = DBConexion.ObtenerConexion())
                {
                    oconexion.Open();
                    var storedProcedure = "sp_Confirmar";
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

        public static UsuarioDTO Obtener(string correo)
        {
            UsuarioDTO usuario = null;
            try
            {
                using (SqlConnection oconexion = DBConexion.ObtenerConexion())
                {
                    string query = "select Nombre,Empresa,Clave,Restablecer,Confirmado,Token from Usuario";
                    query += " where Correo=@correo";

                    var cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@correo", correo);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            usuario = new UsuarioDTO()
                            {
                                Nombre = dr["Nombre"].ToString(),
                                Empresa = dr["Empresa"].ToString(),
                                Clave = dr["Clave"].ToString(),
                                Restablecer = (bool)dr["Restablecer"],
                                Confirmado = (bool)dr["Confirmado"],
                                Token = dr["Token"].ToString()
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

        public static bool Registrar(UsuarioDTO usuario)
        {
            bool respuesta = false;
            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    string query = "INSERT INTO Usuario(Nombre,Empresa,Correo,Clave,Restablecer,Confirmado,Token)";
                    query += " values(@nombre,@empresa,@correo,@clave,@restablecer,@confirmado,@token)";

                    var cmd = new SqlCommand(query, conexion);
                    cmd.Parameters.AddWithValue("@nombre", usuario.Nombre);
                    cmd.Parameters.AddWithValue("@empresa", usuario.Empresa);
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
                        var correo = new CorreoDTO
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
