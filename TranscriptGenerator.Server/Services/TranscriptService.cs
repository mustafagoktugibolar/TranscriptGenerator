using TranscriptGenerator.Server.Models;
using TranscriptGenerator.Server.Services.Interfaces;
using TranscriptGenerator.Server.Shared.Enums;

namespace TranscriptGenerator.Server.Services
{
    public class TranscriptService : ITranscriptService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedExtensions = new[] { ".mp3", ".mp4", ".wav" };

        public TranscriptService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(bool Success, string Result)> TranscribeYoutubeAsync(YoutubeTranscribeRequest request)
        {
            string scriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "transcribe.py");
            string modelArg = GetModelArgument(request);
            string args = $"\"{scriptPath}\" --source youtube --url \"{request.Url}\" {modelArg}";

            int timeout = GetDefaultTimeoutMilliseconds(request.Model);
            var (output, error, code) = await ScriptRunner.RunPythonAsync(args, timeout);

            return code == 0 ? (true, output) : (false, error);
        }

        public async Task<(bool Success, string Result)> TranscribeFileAsync(TranscribeFileRequest request)
        {
            var file = request.File;
            string ext = Path.GetExtension(file.FileName).ToLower();
            if (!_allowedExtensions.Contains(ext))
                return (false, "Invalid file type.");

            string tempFileName = $"temp_{Guid.NewGuid()}{ext}";
            string tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

            using (var stream = new FileStream(tempPath, FileMode.Create))
                await file.CopyToAsync(stream);

            string scriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "transcribe.py");
            string modelArg = GetModelArgument(request);
            string args = $"\"{scriptPath}\" --source file --path \"{tempPath}\" {modelArg}";

            int timeout = GetTimeoutMilliseconds(file.Length, request.Model);
            var (output, error, code) = await ScriptRunner.RunPythonAsync(args, timeout);

            File.Delete(tempPath);

            return code == 0 ? (true, output) : (false, error);
        }

        private string GetModelArgument(ITranscribeRequest request)
        {
            return $"--model {request.Model.ToString().ToLower()}";
        }

        private int GetTimeoutMilliseconds(long fileSizeBytes, WhisperModels model)
        {
            long sizeInMB = fileSizeBytes / (1024 * 1024);
            double multiplier = model switch
            {
                WhisperModels.Tiny => 1.0,
                WhisperModels.Base => 2.0,
                WhisperModels.Small => 3.0,
                WhisperModels.Medium => 4.0,
                WhisperModels.Large => 6.0,
                _ => 2.0
            };
            int estimatedMinutes = (int)Math.Ceiling(sizeInMB * multiplier / 2);
            return Math.Clamp(estimatedMinutes, 1, 30) * 60 * 1000;
        }

        private int GetDefaultTimeoutMilliseconds(WhisperModels model)
        {
            return model switch
            {
                WhisperModels.Tiny => 3 * 60 * 1000,
                WhisperModels.Base => 6 * 60 * 1000,
                WhisperModels.Small => 10 * 60 * 1000,
                WhisperModels.Medium => 15 * 60 * 1000,
                WhisperModels.Large => 25 * 60 * 1000,
                _ => 6 * 60 * 1000
            };
        }
    }
}
