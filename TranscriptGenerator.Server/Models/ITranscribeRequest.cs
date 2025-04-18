using TranscriptGenerator.Server.Shared.Enums;

namespace TranscriptGenerator.Server.Models
{
    public interface ITranscribeRequest
    {
        WhisperModels Model { get; set; }
    }

}
