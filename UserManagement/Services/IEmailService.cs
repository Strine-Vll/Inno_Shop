using UserManagement.Models;

namespace UserManagement.Services
{
    public interface IEmailService
    {
        Task SendEmail(EmailDto request);
    }
}
