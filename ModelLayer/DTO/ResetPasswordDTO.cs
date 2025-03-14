using System;
namespace ModelLayer.DTO
{
    public class ResetPasswordDTO
    {
        public string Email { get; set; }
        public string NewPassword { get; set; }
        public string ResetToken { get; set; }
    }
}
