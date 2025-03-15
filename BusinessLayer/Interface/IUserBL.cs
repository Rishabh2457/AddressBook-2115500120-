using ModelLayer.DTO;
using ModelLayer.Model;

namespace BusinessLayer.Interface
{
    public interface IUserBL
    {
        User RegisterUser(RegisterDTO userRegisterDTO);
        UserResponseDTO LoginUser(LoginDTO loginDTO);
        bool ForgetPassword(string email);
        bool ResetPassword(string token, string newPassword);
        List<RegisterDTO> GetAllUsers();
    }
}