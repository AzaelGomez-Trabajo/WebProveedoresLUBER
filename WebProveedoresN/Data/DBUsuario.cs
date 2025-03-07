using WebProveedoresN.Models;
using System.Data;
using System.Data.SqlClient;
using WebProveedoresN.Conexion;
using Microsoft.Data.SqlClient;

namespace WebProveedoresN.Data
{
    public class DBUsuario
    {
        public List<UsuarioModel> Listar()
        {
            var lista = new List<UsuarioModel>();
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var storedProcedure = "sp_Listar";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                using (var dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        var usuario = new UsuarioModel
                        {
                            Id = Convert.ToInt32(dr["Id"]),
                            Empresa = dr["Empresa"].ToString(),
                            Nombre = dr["Nombre"].ToString(),
                            Correo = dr["Correo"].ToString(),
                            Clave = dr["Clave"].ToString(),
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                        };
                        lista.Add(usuario);
                    }
                }
            }
            return lista;
        }

        public UsuarioModel Obtener(int id)
        {
            var usuario = new UsuarioModel();
            using (var conexion = DBConexion.ObtenerConexion())
            {
                conexion.Open();
                var storedProcedure = "sp_Obtener";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Id", id);
                using (var dr = cmd.ExecuteReader())
                {
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
                        usuario.IdState = Convert.ToInt32(dr["IdState"]);
                    }
                }
            }
            return usuario;
        }

        public bool Guardar(UsuarioModel usuario)
        {
            bool rpta = false;

            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
                    conexion.Open();
                    var storedProcedure = "sp_Guardar";
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
                        cmd.Parameters.AddWithValue("@IdState", usuario.IdState);
                        cmd.ExecuteNonQuery();
                    }
                    rpta = true;
                }
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

        public bool Editar(UsuarioModel usuario)
        {
            bool rpta = false;

            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
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
                        cmd.Parameters.AddWithValue("@IdState", usuario.IdState);
                        cmd.ExecuteNonQuery();
                    }
                    rpta = true;
                }
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

        public bool ChangeStatus(string Token, int Status)
        {
            bool rpta = false;

            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
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

        public bool ChangeAccess(string Token, int Access)
        {
            bool rpta = false;

            try
            {
                using (var conexion = DBConexion.ObtenerConexion())
                {
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
