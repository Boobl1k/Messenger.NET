using MyBook.Services.EmailServices;

namespace MyBook.Services.EmailServices;

public interface IEmailService
{
    void SendEmail(Message message);
    Task SendEmailAsync(Message message);

}