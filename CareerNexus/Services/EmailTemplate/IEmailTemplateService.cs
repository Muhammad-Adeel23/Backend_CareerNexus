using CareerNexus.Models.EmailTemplate;

namespace CareerNexus.Services.EmailTemplate
{
    public interface IEmailTemplateService
    {
        Task<EmailTemplateModel> GetEmailTemplateById(long templateEnum);
    }
}
