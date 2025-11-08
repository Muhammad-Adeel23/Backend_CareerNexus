namespace CareerNexus.Models.Resume
{
    public class ResumeUploadRequest
    {
        public IFormFile ResumeFile { get; set; } = null!;
        //public long UserId { get; set; }
    }
}
