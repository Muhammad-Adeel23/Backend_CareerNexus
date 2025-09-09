using CareerNexus.Common;
using CareerNexus.Models.EmailTemplate;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Net;
using System.Net.Mail;

namespace CareerNexus.Services.EmailTemplate
{
    public class EmailTemplateService:IEmailTemplateService
    {
        private readonly ILogger<EmailTemplateService> _Logger;

        public EmailTemplateService(ILogger<EmailTemplateService> logger)
        {
            _Logger = logger;                    
        }
        public async Task<EmailTemplateModel> GetEmailTemplateById(long templateEnum)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                string query = "Select  Subject, TemplateBody from InvitationTemplates where SeqKey =@SeqKey";
                cmd.CommandText = query;
                cmd.CommandType = CommandType.Text;
                cmd.Parameters.AddWithValue("@SeqKey", (long)templateEnum);
                DataTable dt = DBEngine.GetDataTable(cmd, Databaseoperations.Select, query);
                if (dt != null && dt.Rows.Count > 0)
                {
                    return new EmailTemplateModel
                    {
                        Subject = dt.Rows[0]["Subject"].ToString(),
                        TemplateBody = dt.Rows[0]["TemplateBody"].ToString()
                    };
                }

                return null;
            }

            catch (Exception ex)
            {
                _Logger.LogError($"Fetch Projects: Error occured while Fetching project, Error: {ex.Message}, StackTrace: {ex.StackTrace}");
                return null;
            }
        }
    }
}
