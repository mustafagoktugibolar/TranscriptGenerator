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
            string ytScriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "youtube_to_mp3.py");
            string transcribeScriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "transcribe.py");
            string modelArg = GetModelArgument(request);

            string ytArgs = $"\"{ytScriptPath}\" --url \"{request.Url}\"";
            int timeOutms = 2 * 60 * 1000; // if it takes more than 2 minutes kill
            var (mp3Path, ytError, ytExitCode) = await ScriptRunner.RunPythonAsync(ytArgs, timeOutms);

            if (ytExitCode != 0 || string.IsNullOrWhiteSpace(mp3Path))
            {
                return (false, $"YouTube download failed: {ytError}");
            }

            mp3Path = mp3Path.Trim();

            string transcribeArgs = $"\"{transcribeScriptPath}\" --path \"{mp3Path}\" {modelArg}";
            int timeout = GetDefaultTimeoutMilliseconds(request.Model);

            string output = string.Empty;
            string transcribeError = string.Empty;
            int code = -1;

            try
            {
                var result = await ScriptRunner.RunPythonAsync(transcribeArgs, timeout);
                output = result.output?.Trim();
                transcribeError = result.error;
                code = result.exitCode;
            }
            catch (Exception ex)
            {
                transcribeError = "Transcription process crashed: " + ex.Message;
            }
            finally
            {
                try
                {
                    if (File.Exists(mp3Path))
                    {
                        File.Delete(mp3Path);
                        Console.WriteLine($"Deleted temp file: {mp3Path}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to delete temp mp3 file: " + ex.Message);
                }
            }

            return code == 0
                ? (true, output)
                : (false, transcribeError ?? "Unknown error during transcription");
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
            string args = $"\"{scriptPath}\" --path \"{tempPath}\" {modelArg}";

            int timeout = GetTimeoutMilliseconds(file.Length, request.Model);
            var (output, error, code) = await ScriptRunner.RunPythonAsync(args, timeout);

            try { File.Delete(tempPath); } catch { }

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
            return Math.Clamp(estimatedMinutes, 1, 120) * 60 * 1000;
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
