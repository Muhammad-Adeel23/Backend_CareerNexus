using CareerNexus.Common;
using CareerNexus.Models.ChangePassword;
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

        public async Task<(bool, string)> ChangePassword(ChangePassword changePassword)
        {
            string message = string.Empty; try
            {
                string query = @"SELECT Id FROM USERS WHERE Id = @userId AND PasswordHash = @oldPassword";
                SqlCommand sqlCmd = new SqlCommand();
                sqlCmd.Parameters.AddWithValue("@UserId", changePassword.UserId);
                sqlCmd.Parameters.AddWithValue("@oldPassword", Helper.EncryptString(changePassword.OldPassword));
                sqlCmd.CommandText = query;
                sqlCmd.CommandType = CommandType.Text; // Execute the SQL command and return the result.
               DataTable dt = DBEngine.GetDataTable(sqlCmd,Databaseoperations.Select, query);
                if (dt.Rows.Count == 0) {
                    message = "User or password is incorrect";
                    _logger.LogError($"User or password is incorrect.");
                    return (false, message); 
                } 
                else {
                    try {
                        query = @"UPDATE Users SET PasswordHash = @newPassword, UpdatedOn = GETDATE(), UpdatedBy = @userId WHERE Id = @userId";
                        sqlCmd = new SqlCommand();
                        sqlCmd.Parameters.AddWithValue("@newPassword", Helper.EncryptString(changePassword.NewPassword));
                        sqlCmd.Parameters.AddWithValue("@userId", Convert.ToInt64(dt.Rows[0]["Id"]));
                        sqlCmd.CommandText = query;
                        sqlCmd.CommandType = CommandType.Text;
                        var rowsaffected = DBEngine.ExecuteScalar(sqlCmd, Databaseoperations.Update, query);
                        if (rowsaffected == null) { 
                            message = "Password update failed.";
                            _logger.LogError("Password update failed.");
                            return (false, message);
                        }         message = "Password update successfully.";
                        return (true, "");
                    }
                    catch (Exception ex) {
                        _logger.LogError($"Error occured changing password: {ex.Message}");
                        message = $"Error occured changing password: {ex.Message}";
                        return (false, message);
                    } 
                }
            }
            catch (Exception ex) { 
                _logger.LogError($"ChangePassword: Error occured while changing password, Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                message = $"ChangePassword: Error occured while changing password, Error: {ex.Message}, StackTrace: {ex.StackTrace}";
                return (false, message);
            } 
        }
            }
}
