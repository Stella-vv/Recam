namespace Remp.API.Services.Email;

public interface IEmailSender
{
    public Task SendEmailAsync(string title, string body, string receiver);
}