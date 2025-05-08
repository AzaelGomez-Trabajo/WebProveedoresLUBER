using WebProveedoresN.DTOs;
using WebProveedoresN.Models;

namespace WebProveedoresN.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<bool> ValidateFirstUserAsync(SupplierModel supplier);
        
        Task<bool> ValidateSupplierAsync(SupplierModel supplier);

        Task<string> SaveUserWithRolesAsync(UserModel usuario);

        Task<string> ConfirmEmailAsync(string token);

        Task<UserModel> ValidateUserAsync(string email, string password);

        Task<string> ResetPasswordAsync(UpdateDTO updateDTO);

        Task<string> UpdatePasswordAsync(UpdateDTO updateDTO);

        Task<List<UserModel>> ListarUsuariosConRolesAsync(string supplierCode);

        Task<UserModel> GetUserByTokenAsync(string token);

        Task<string> SaveGuestWithRolesAsync(UserModel usuario);

        Task<string> UpdateUserAsync(UpdateUserDTO UpdateUserDTO);

        Task<GetUserDTO> GetUserAsync(EmailDTO emailDTO);

        Task<string> UpdateProfileAsync(UserProfileDTO userProfileDTO);
    }
}
