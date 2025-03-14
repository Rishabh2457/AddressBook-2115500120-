using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;

        //constructor of class
        public UserBL(IUserRL userRL)
        {
            _userRL = userRL;
        }
        //method to register the user
        public User RegisterUser(RegisterDTO userRegisterDTO)
        {
            return _userRL.RegisterUser(userRegisterDTO);
        }
        //method to login the user
        public UserResponseDTO LoginUser(LoginDTO loginDTO)
        {
            return _userRL.LoginUser(loginDTO);
        }
        //method to get the token on mail for forget password
        public bool ForgetPassword(string email)
        {
            return _userRL.ForgetPassword(email);
        }
        //method to reset the password 
        public bool ResetPassword(string token, string newPassword)
        {
            return _userRL.ResetPassword(token, newPassword);
        }
        public List<UserResponseDTO> GetAllUsers()
        {
            var users = _userRL.GetAllUsers();
            return users.Select(u => new UserResponseDTO
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                UserRole = u.UserRole
            }).ToList();
        }


    }
}