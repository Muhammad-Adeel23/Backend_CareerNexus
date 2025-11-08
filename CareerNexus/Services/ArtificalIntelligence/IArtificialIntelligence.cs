namespace CareerNexus.Services.ArtificalIntelligence
{
    public interface IArtificialIntelligence
    {
        Task<string> OpenAITurboModelAsync(string prompt, string resumeText);
    }
}
