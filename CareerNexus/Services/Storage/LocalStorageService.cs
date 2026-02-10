namespace CareerNexus.Services.Storage
{
    public class LocalStorageService : IStorageService
    {
        private readonly string _basePath;
        public LocalStorageService(IWebHostEnvironment env)
        {
            _basePath = Path.Combine(env.WebRootPath ?? "wwwroot", "uploads");
            Directory.CreateDirectory(_basePath);
        }

        //public async Task<string> SaveFileAsync(IFormFile file, long? userId)
        //{
        //    var userFolder = Path.Combine(_basePath, userId.ToString());
        //    Directory.CreateDirectory(userFolder);
        //    var safeName = Path.GetFileName(file.FileName);
        //    var filePath = Path.Combine(userFolder, safeName);

        //    using (var fs = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(fs);
        //    }


        //    return safeName;
        //}
        public async Task<string> SaveFileAsync(IFormFile file, long? userId)
        {
            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "resumes", userId.ToString());

            Directory.CreateDirectory(uploadsFolder);

            var safeName = Path.GetFileName(file.FileName);

            var filePath = Path.Combine(uploadsFolder, safeName);

            using (var fs = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }


            return Path.Combine("resumes", userId.ToString(), safeName).Replace("\\", "/");
        }
        //public async Task<string> SaveFileAsync(IFormFile file, long? userId)
        //{
        //    var userFolder = Path.Combine(_basePath, userId.ToString());
        //    Directory.CreateDirectory(userFolder);
        //    var safeName = Path.GetFileName(file.FileName);
        //    var filePath = Path.Combine(userFolder, safeName);

        //    using (var fs = new FileStream(filePath, FileMode.Create))
        //    {
        //        await file.CopyToAsync(fs);
        //    }


        //    return safeName;
        //}
    }

}
