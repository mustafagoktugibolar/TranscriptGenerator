using TranscriptGenerator.Server.Models;

namespace TranscriptGenerator.Server.Services.Interfaces
{
    public interface ITranscriptService
    {
        Task<(bool Success, string Result)> TranscribeYoutubeAsync(YoutubeTranscribeRequest request);
        Task<(bool Success, string Result)> TranscribeFileAsync(TranscribeFileRequest request);
    }
}
