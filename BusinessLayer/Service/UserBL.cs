using BusinessLayer.Interface;
using RepositoryLayer.Interface;
using ModelLayer.DTO;
using ModelLayer.Model;


namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly IUserRL _userRL;
        private readonly IRabbitMqProducer _rabbitMqProducer;

        //constructor of class
        public UserBL(IUserRL userRL, IRabbitMqProducer rabbitMqProducer)
        {
            _userRL = userRL;
            _rabbitMqProducer = rabbitMqProducer;
        }
        //method to register the user
        public User RegisterUser(RegisterDTO userRegisterDTO)
        {
            var user = _userRL.RegisterUser(userRegisterDTO);
            if (user != null)
            {
                var userEvent = new UserEventDTO
                {
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    EventType = "UserRegistered"
                };
                _rabbitMqProducer.PublishMessage(userEvent);
            }
            return user;
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

        public List<RegisterDTO> GetAllUsers()
        {
            var users = _userRL.GetAll();
            return users.Select(u => new RegisterDTO
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email
            }).ToList();
        }

    }
}