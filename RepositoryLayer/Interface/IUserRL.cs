using System;
using ModelLayer.Model;
using ModelLayer.DTO;
namespace RepositoryLayer.Interface
{
    public interface IUserRL
    {
        User RegisterUser(RegisterDTO registerDTO);
        UserResponseDTO LoginUser(LoginDTO loginDTO);
        //string GenerateJwtToken(User user);
        bool ResetPassword(string token, string newPassword);
        bool ForgetPassword(string email);

        List<User> GetAllUsers();

    }
}
