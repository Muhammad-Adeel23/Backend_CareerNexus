using CareerNexus.Common;
using CareerNexus.Services.EmailSender;
using CareerNexus.Services.EmailTemplate;
using CareerNexus.Services.Setting;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CareerNexus.Services.User
{
    public class UserService:IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IEmailTemplateService _emailTemplate;
        private readonly IEmailSenderService _emailSender;
        public UserService(ILogger<UserService> logger,IEmailSenderService emailSender,IEmailTemplateService emailTemplate)
        {
            _emailSender = emailSender;
            _emailTemplate = emailTemplate;
            _logger = logger;
        }
        public static string GenerateRandomPassword(int length = 12)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%^&*";
            var rand = new Random();
            return new string(Enumerable.Range(0, length).Select(_ => chars[rand.Next(chars.Length)]).ToArray());
        }
        public async Task<bool> ForgotPassword(string email)
        {
            try
            {
                string generatedPassword = GenerateRandomPassword();
                string query = @"UPDATE Users SET PasswordHash = @password where email = @email";
                SqlCommand sqlCmd = new SqlCommand();

                sqlCmd.Parameters.AddWithValue("@password", Helper.EncryptString(generatedPassword));
                sqlCmd.Parameters.AddWithValue("@email", email);
                sqlCmd.CommandText = query;
                sqlCmd.CommandType = CommandType.Text;
                // Execute the SQL command and return the result.
                bool flag = DBEngine.ExecuteNonQuery(sqlCmd, Databaseoperations.Update, query);
                if (!flag)
                {
                    _logger.LogInformation($"User not found to be reset.");
                    return false;
                }
                var emailTemplate = await _emailTemplate.GetEmailTemplateById((long)EmailTemplateEnum.PasswordReset);
                if (emailTemplate == null)
                {
                    _logger.LogWarning("Password reset email template not found.");
                    return false;
                }

                string body = emailTemplate.TemplateBody
                               .Replace("{{Email}}", email)
                               .Replace("{{Password}}", generatedPassword);
                string FromEmail = SettingService.GetSettingsKeyValue(SystemVariables.SenderEmail);
                string Subject = emailTemplate.Subject;

                //Send OTP through Email to the User
              //  await _EmailSender.SendEmailAsync(FromEmail, user.Email, Subject, body);
                await _emailSender.SendEmailAsync(FromEmail, email, "Password Reset", body);
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError($"ForgotPassword: Error occured while resetting password, Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
