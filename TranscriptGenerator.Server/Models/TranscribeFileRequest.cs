using TranscriptGenerator.Server.Shared.Enums;

namespace TranscriptGenerator.Server.Models
{
    public class TranscribeFileRequest : ITranscribeRequest
    {
        public IFormFile File { get; set; } = default!;
        public WhisperModels Model { get; set; } = WhisperModels.Base;
    }
}
