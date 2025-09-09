namespace CareerNexus.Services.EmailSender
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body);
    }
}
