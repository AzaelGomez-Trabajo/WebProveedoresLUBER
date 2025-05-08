using Microsoft.Data.SqlClient;
using System.Data;
using WebProveedoresN.Data;
using WebProveedoresN.DTOs;
using WebProveedoresN.Models;
using WebProveedoresN.Repositories.Interfaces;

namespace WebProveedoresN.Repositories.Implementations
{
    public class UserRepository : IUserRepository
    {
        private readonly DBConnection _dBConnection;

        public UserRepository(DBConnection dBConnection)
        {
            _dBConnection = dBConnection;
        }
        public async Task<bool> ValidateFirstUserAsync(SupplierModel supplier)
        {
            using var conexion = await _dBConnection.GetConnectionAsync();
            await conexion.OpenAsync();
            var storedProcedure = "sp_ValidateFirstUser";
            using var cmd = new SqlCommand(storedProcedure, conexion);
            cmd.Parameters.AddWithValue("@Code", supplier.Code);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.CommandType = CommandType.StoredProcedure;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public async Task<bool> ValidateSupplierAsync(SupplierModel supplier)
        {
            using var conexion = await _dBConnection.GetConnectionAsync();
            await conexion.OpenAsync();
            var storedProcedure = "sp_ValidateSupplier";
            using var cmd = new SqlCommand(storedProcedure, conexion);
            cmd.Parameters.AddWithValue("@Code", supplier.Code);
            cmd.Parameters.AddWithValue("@Name", supplier.Name);
            cmd.CommandType = CommandType.StoredProcedure;
            var result = await cmd.ExecuteScalarAsync();
            return Convert.ToBoolean(result);
        }

        public async Task<string> SaveUserWithRolesAsync(UserModel usuario)
        {
            using var conexion = await _dBConnection.GetConnectionAsync();
            await conexion.OpenAsync();
            var transaction = await conexion.BeginTransactionAsync();
            var storedProcedure = "sp_SaveUserWithRoles";
            try
            {
                using var cmd = new SqlCommand(storedProcedure, conexion, (SqlTransaction)transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.FullName);
                cmd.Parameters.AddWithValue("@SupplierCode", usuario.SupplierCode);
                cmd.Parameters.AddWithValue("@Correo", usuario.Email);
                cmd.Parameters.AddWithValue("@Clave", usuario.Password);
                cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.Parameters.AddWithValue("@StatusId", usuario.IdStatus);
                var idUsuario = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                //foreach (var rol in usuario.Roles)
                //{
                storedProcedure = "sp_SaveUserRole";
                cmd.CommandText = storedProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UsuarioId", idUsuario);
                cmd.Parameters.AddWithValue("@RoleId", 1);
                await cmd.ExecuteNonQueryAsync();
                //}
                await transaction.CommitAsync();

                return "Usuario guardado exitosamente.";
            }
            catch (SqlException ex)
            {
                await transaction.RollbackAsync();
                var error = ex.Message;
                return $"Error al guardar los datos en SQL Server - {error}";
            }
            catch (Exception ex)
            {
                var error = ex.Message;
                return $"Error inesperado al guardar los datos en SQL Server - {error}";
            }
        }

        public async Task<string> ConfirmEmailAsync(string token)
        {
            try
            {
                using (var oconexion = await _dBConnection.GetConnectionAsync())
                {
                    await oconexion.OpenAsync();
                    var storedProcedure = "sp_ConfirmEmail";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@token", token);
                    await cmd.ExecuteNonQueryAsync();
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

        public async Task<UserModel> ValidateUserAsync(string email, string password)
        {
            var usuario = new UserModel();
            try
            {
                using (var conexion = await _dBConnection.GetConnectionAsync())
                {
                    await conexion.OpenAsync();
                    var storedProcedure = "sp_ValidateUser";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@correo", email);
                    cmd.Parameters.AddWithValue("@clave", password);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = await cmd.ExecuteReaderAsync();
                    while (dr.Read())
                    {
                        usuario = new UserModel
                        {
                            IdUsuario = Convert.ToInt32(dr["Id"]),
                            SupplierCode = dr["Code"].ToString()!,
                            SupplierName = dr["Name"].ToString()!,
                            FullName = dr["Nombre"].ToString()!,
                            Email = email,
                            Password = password,
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

        public async Task<string> ResetPasswordAsync(UpdateDTO updateDTO)
        {
            try
            {
                using (var oconexion = await _dBConnection.GetConnectionAsync())
                {
                    await oconexion.OpenAsync();
                    var storedProcedure = "sp_ResetPassword";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Restablecer", updateDTO.Restablecer);
                    cmd.Parameters.AddWithValue("@Clave", updateDTO.Password);
                    cmd.Parameters.AddWithValue("@Token", updateDTO.Token);
                    await cmd.ExecuteNonQueryAsync();
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

        public async Task<string> UpdatePasswordAsync(UpdateDTO updateDTO)
        {
            try
            {
                using (var oconexion = await _dBConnection.GetConnectionAsync())
                {
                    await oconexion.OpenAsync();
                    var storedProcedure = "sp_UpdatePassword";
                    using var cmd = new SqlCommand(storedProcedure, oconexion);
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Restablecer", updateDTO.Restablecer);
                    cmd.Parameters.AddWithValue("@Clave", updateDTO.Password);
                    cmd.Parameters.AddWithValue("@Token", updateDTO.Token);
                    await cmd.ExecuteNonQueryAsync();
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

        public async Task<List<UserModel>> ListarUsuariosConRolesAsync(string supplierCode)
        {
            var users = new List<UserModel>();
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                await conexion.OpenAsync();
                var storedProcedure = "sp_ListarUsuariosConRoles";
                var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.Parameters.AddWithValue("@SupplierCode", supplierCode);
                cmd.CommandType = CommandType.StoredProcedure;
                using var dr = await cmd.ExecuteReaderAsync();
                while (dr.Read())
                {
                    var user = new UserModel
                    {
                        FullName = dr["Nombre"].ToString()!,
                        Email = dr["Correo"].ToString()!,
                        Password = dr["Clave"].ToString()!,
                        Token = dr["Token"].ToString()!,
                        Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                        Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                        IdStatus = Convert.ToInt32(dr["StatusId"]),
                        Roles = new List<string>()
                    };
                    if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                    {
                        user.Roles.Add(dr["Rol"].ToString()!);
                    }
                    users.Add(user);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return users;
        }

        public async Task<UserModel> GetUserByTokenAsync(string token)
        {
            var user = new UserModel();
            try
            {
                using (var conexion = await _dBConnection.GetConnectionAsync())
                {
                    await conexion.OpenAsync();
                    var storedProcedure = "sp_GetUserByToken";
                    var cmd = new SqlCommand(storedProcedure, conexion);
                    cmd.Parameters.AddWithValue("@Token", token);
                    cmd.CommandType = CommandType.StoredProcedure;
                    using var dr = await cmd.ExecuteReaderAsync();
                    while (dr.Read())
                    {
                        user = new UserModel
                        {
                            IdUsuario = Convert.ToInt32(dr["Id"]),
                            FullName = dr["Nombre"].ToString()!,
                            SupplierCode = dr["SupplierCode"].ToString()!,
                            SupplierName = dr["Empresa"].ToString()!,
                            Email = dr["Correo"].ToString()!,
                            Password = dr["Clave"].ToString()!,
                            Token = dr["Token"].ToString()!,
                            Restablecer = Convert.ToBoolean(dr["Restablecer"]),
                            Confirmado = Convert.ToBoolean(dr["Confirmado"]),
                            IdStatus = Convert.ToInt32(dr["StatusId"]),
                            Roles = []
                        };
                        if (!dr.IsDBNull(dr.GetOrdinal("Rol")))
                        {
                            user.Roles.Add(dr["Rol"].ToString()!);
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return user;
        }

        public async Task<string> SaveGuestWithRolesAsync(UserModel usuario)
        {
            using var conexion = await _dBConnection.GetConnectionAsync();
            await conexion.OpenAsync();
            var transaction = conexion.BeginTransaction();
            var storedProcedure = "sp_SaveUserWithRoles";
            try
            {
                using var cmd = new SqlCommand(storedProcedure, conexion, transaction);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Nombre", usuario.FullName);
                cmd.Parameters.AddWithValue("@SupplierCode", usuario.SupplierCode);
                cmd.Parameters.AddWithValue("@Correo", usuario.Email);
                cmd.Parameters.AddWithValue("@Clave", usuario.Password);
                cmd.Parameters.AddWithValue("@Restablecer", usuario.Restablecer);
                cmd.Parameters.AddWithValue("@Confirmado", usuario.Confirmado);
                cmd.Parameters.AddWithValue("@Token", usuario.Token);
                cmd.Parameters.AddWithValue("@StatusId", usuario.IdStatus);
                var idUsuario = Convert.ToInt32(await cmd.ExecuteScalarAsync());

                //foreach (var rol in usuario.Roles)
                //{
                storedProcedure = "sp_SaveUserRole";
                cmd.CommandText = storedProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@UsuarioId", idUsuario);
                cmd.Parameters.AddWithValue("@RoleId", 2);
                await cmd.ExecuteNonQueryAsync();
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

        public async Task<string> UpdateUserAsync(UpdateUserDTO updateUserDTO)
        {
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                await conexion.OpenAsync();
                var storedProcedure = "sp_UpdateUser";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FullName", updateUserDTO.FullName);
                cmd.Parameters.AddWithValue("@Email", updateUserDTO.Email);
                cmd.Parameters.AddWithValue("@StatusId", updateUserDTO.StatusId);
                cmd.Parameters.AddWithValue("@Token", updateUserDTO.Token);
                await cmd.ExecuteNonQueryAsync();
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

        public async Task<GetUserDTO> GetUserAsync(EmailDTO emailDTO)
        {
            GetUserDTO user = null!;
            try
            {
                using (SqlConnection oconexion = await _dBConnection.GetConnectionAsync())
                {
                    await oconexion.OpenAsync();
                    string query = "SELECT Nombre, Clave, Restablecer, Confirmado, Token FROM Usuario";
                    query += " where Correo = @email";

                    var cmd = new SqlCommand(query, oconexion);
                    cmd.Parameters.AddWithValue("@email", emailDTO.Email);
                    cmd.CommandType = CommandType.Text;

                    oconexion.Open();

                    using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                    if (dr.Read())
                    {
                        user = new GetUserDTO()
                        {
                            FullName = dr["Nombre"].ToString()!,
                            Password = dr["Clave"].ToString()!,
                            Restablecer = (bool)dr["Restablecer"],
                            Confirmado = (bool)dr["Confirmado"],
                            Token = dr["Token"].ToString()!
                        };
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return user;
        }

        public async Task<string> UpdateProfileAsync(UserProfileDTO userProfileDTO)
        {
            try
            {
                using var conexion = await _dBConnection.GetConnectionAsync();
                await conexion.OpenAsync();
                var storedProcedure = "sp_UpdateProfile";
                using var cmd = new SqlCommand(storedProcedure, conexion);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FullName", userProfileDTO.FullName);
                cmd.Parameters.AddWithValue("@Email", userProfileDTO.Email);
                cmd.Parameters.AddWithValue("@Token", userProfileDTO.Token);
                await cmd.ExecuteNonQueryAsync();
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