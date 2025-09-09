using CareerNexus.Common;
using CareerNexus.Services.Setting;
using System.Net;
using System.Net.Mail;

namespace CareerNexus.Services.EmailSender
{
    public class EmailSenderService:IEmailSenderService
    {
        private readonly ILogger<EmailSenderService> _logger;
        public EmailSenderService(ILogger<EmailSenderService> logger)
        {
            _logger = logger;
        }
        public async Task SendEmailAsync(string fromEmail, string toEmail, string subject, string body)
        {
            
                var smtpClient = new SmtpClient(Convert.ToString(SettingService.GetSettingsKeyValue(SystemVariables.SMTPServer)), Convert.ToInt32(SettingService.GetSettingsKeyValue(SystemVariables.SMTPPort)))
                {
                    EnableSsl = Convert.ToBoolean(SettingService.GetSettingsKeyValue(SystemVariables.IsTLS)),
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromEmail, SettingService.GetSettingsKeyValue(SystemVariables.SenderEmailPassword))
                };

                var mailMessage = new MailMessage(fromEmail, toEmail, subject, body);
                mailMessage.IsBodyHtml = true;
                await smtpClient.SendMailAsync(mailMessage);
            
            
        }
    }
}
