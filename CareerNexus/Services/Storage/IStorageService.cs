namespace CareerNexus.Services.Storage
{
    public interface IStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, long?userId);
    }
}
