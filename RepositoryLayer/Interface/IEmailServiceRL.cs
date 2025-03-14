using System;
namespace RepositoryLayer.Interface
{
    public interface IEmailService
    {
        void SendEmail(string toEmail, string subject, string body);
    }
}
