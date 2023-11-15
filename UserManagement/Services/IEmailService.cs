using UserManagement.Dtos;

namespace UserManagement.Services
{
    public interface IEmailService
    {
        Task SendEmail(EmailDto request);
    }
}
