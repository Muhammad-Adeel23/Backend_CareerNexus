namespace CareerNexus.Models.Resume
{
    public class ResumeUploadRequest
    {
        public IFormFile ResumeFile { get; set; } = null!;
        public Guid? TempSessionId { get; set; }
        //public long UserId { get; set; }
    }
}
