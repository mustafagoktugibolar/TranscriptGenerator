using TranscriptGenerator.Server.Shared.Enums;

namespace TranscriptGenerator.Server.Models
{
    public class YoutubeTranscribeRequest : ITranscribeRequest
    {
        public string Url { get; set; } = string.Empty;
        public WhisperModels Model { get; set; } = WhisperModels.Base;
    }
}
