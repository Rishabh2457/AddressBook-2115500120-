using System;
using RepositoryLayer.Interface;
using RepositoryLayer.Context;
using ModelLayer.Model;
using ModelLayer.DTO;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using RepositoryLayer.Hashing;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.Service
{
    public class UserRL : IUserRL
    {
        private readonly AddressBookContext _context;
        private readonly PasswordHash _passwordHash;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        public UserRL(AddressBookContext context, PasswordHash hash, IEmailService emailService, IConfiguration configuration)
        {
            _context = context;
            _passwordHash = hash;
            _emailService = emailService;
            _configuration = configuration;
        }
        /// <summary>
        /// Method to register the user in database
        /// </summary>
        /// <param name="userRegisterDTO">User data </param>
        /// <returns>User(info) or null </returns>
        public User RegisterUser(RegisterDTO userRegisterDTO)
        {
            var existingUser = _context.Users.FirstOrDefault(e => e.Email == userRegisterDTO.Email);
            if (existingUser == null)
            {
                var newUser = new User
                {
                    FirstName = userRegisterDTO.FirstName,
                    LastName = userRegisterDTO.LastName,
                    Email = userRegisterDTO.Email,
                    UserRole = userRegisterDTO.UserRole,
                    PasswordHash = _passwordHash.PasswordHashing(userRegisterDTO.Password)
                };
                _context.Users.Add(newUser);
                _context.SaveChanges();
                return newUser;
            }

            return null;
        }
        /// <summary>
        /// method to login the user
        /// </summary>
        /// <param name="loginDTO">email and password from user</param>
        /// <returns>return user info with token  or null</returns>
        public UserResponseDTO LoginUser(LoginDTO loginDTO)
        {
            var validUser = _context.Users.FirstOrDefault(e => e.Email == loginDTO.Email);
            if (validUser != null && _passwordHash.VerifyPassword(loginDTO.Password, validUser.PasswordHash))
            {
                return new UserResponseDTO
                {
                    FirstName = validUser.FirstName,
                    LastName = validUser.LastName,
                    Email = validUser.Email,
                    UserRole = validUser.UserRole,
                    Token = GenerateJwtToken(validUser)

                };

            }
            return null;
        }

        /// <summary>
        /// method to generate the jwt token
        /// </summary>
        /// <param name="user">user info from database</param>
        /// <returns>token </returns>
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim("Id", user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.UserRole.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public List<User> GetAllUsers()
        {
            return _context.Users.ToList();
        }

        /// <summary>
        /// method to get the temperory token to verify the user while reseting the password
        /// </summary>
        /// <returns>token</returns>
        public string GenerateResetToken()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
        /// <summary>
        /// method to get the token on mail to reset the password
        /// </summary>
        /// <param name="email">email address of the user</param>
        /// <returns>token on email</returns>
        public bool ForgetPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null)
            {
                return false;
            }
            user.ResetToken = GenerateResetToken();
            user.ResetTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();
            string emailBody = $"Reset Token :\n {user.ResetToken}";
            _emailService.SendEmail(user.Email, "Reset Password", emailBody);
            return true;

        }
        /// <summary>
        /// method to change the password
        /// </summary>
        /// <param name="token">system generated token recieved on mail</param>
        /// <param name="newPassword"> new password </param>
        /// <returns>Success or failure response</returns>
        public bool ResetPassword(string token, string newPassword)
        {
            var user = _context.Users.FirstOrDefault(e => e.ResetToken == token && e.ResetTokenExpiry > DateTime.UtcNow);
            if (user == null)
            {
                //Invalid token
                return false;
            }
            user.PasswordHash = _passwordHash.PasswordHashing(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            _context.SaveChanges();
            return true;
        }

    }
}
