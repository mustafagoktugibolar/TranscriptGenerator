using TranscriptGenerator.Server.Models;
using TranscriptGenerator.Server.Services.Interfaces;
using TranscriptGenerator.Server.Shared.Enums;
using TranscriptGenerator.Server.Helpers;

namespace TranscriptGenerator.Server.Services
{
    public class TranscriptService : ITranscriptService
    {
        private readonly IWebHostEnvironment _env;
        private readonly string[] _allowedExtensions = new[] { ".mp3", ".mp4", ".wav" };
        private const int OneMinuteAsMs = 60000;
        private const int OneMegabyteInBytes = 1048576;

        public TranscriptService(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<(bool Success, string Result)> TranscribeYoutubeAsync(YoutubeTranscribeRequest request)
        {
            LogHelper.Info<TranscriptService>("Starting YouTube transcription for URL: {Url} with model: {Model}", request.Url, request.Model);

            string ytScriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "youtube_to_mp3.py");
            string transcribeScriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "transcribe.py");
            string modelArg = GetModelArgument(request);

            string ytArgs = $"\"{ytScriptPath}\" --url \"{request.Url}\"";
            int timeOutms = 2 * OneMinuteAsMs;
            var (mp3Path, ytError, ytExitCode) = await ScriptRunner.RunPythonAsync(ytArgs, timeOutms);

            if (ytExitCode != 0 || string.IsNullOrWhiteSpace(mp3Path))
            {
                LogHelper.Warn<TranscriptService>("YouTube download failed with exit code {ExitCode}: {Error}", ytExitCode, ytError);
                return (false, $"YouTube download failed: {ytError}");
            }

            mp3Path = mp3Path.Trim();
            LogHelper.Info<TranscriptService>("YouTube audio downloaded successfully: {Path}", mp3Path);

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
                LogHelper.Error<TranscriptService>(ex, "Exception during transcription process.");
            }
            finally
            {
                try
                {
                    if (File.Exists(mp3Path))
                    {
                        File.Delete(mp3Path);
                        LogHelper.Info<TranscriptService>("Deleted temp file: {File}", mp3Path);
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Error<TranscriptService>(ex, "Failed to delete temp mp3 file: {File}", mp3Path);
                }
            }

            return code == 0
                ? (true, output)
                : (false, transcribeError ?? "Unknown error during transcription");
        }

        public async Task<(bool Success, string Result)> TranscribeFileAsync(TranscribeFileRequest request)
        {
            LogHelper.Info<TranscriptService>("Starting file transcription for file: {FileName}, size: {Size} bytes, model: {Model}", request.File.FileName, request.File.Length, request.Model);

            var file = request.File;
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(ext))
            {
                LogHelper.Warn<TranscriptService>("Invalid file type: {Extension}", ext);
                return (false, "Invalid file type.");
            }

            string tempFileName = $"temp_{Guid.NewGuid()}{ext}";
            string tempPath = Path.Combine(Path.GetTempPath(), tempFileName);

            using (var stream = new FileStream(tempPath, FileMode.Create))
                await file.CopyToAsync(stream);

            LogHelper.Info<TranscriptService>("Saved uploaded file to: {Path}", tempPath);

            string scriptPath = Path.Combine(_env.ContentRootPath, "Scripts", "transcribe.py");
            string modelArg = GetModelArgument(request);
            string args = $"\"{scriptPath}\" --path \"{tempPath}\" {modelArg}";

            int timeout = GetTimeoutMilliseconds(file.Length, request.Model);
            var (output, error, code) = await ScriptRunner.RunPythonAsync(args, timeout);

            try
            {
                File.Delete(tempPath);
                LogHelper.Info<TranscriptService>("Deleted temp file: {File}", tempPath);
            }
            catch (Exception ex)
            {
                LogHelper.Error<TranscriptService>(ex, "Failed to delete temp file: {File}", tempPath);
            }

            return code == 0 ? (true, output) : (false, error);
        }

        private string GetModelArgument(ITranscribeRequest request)
        {
            return $"--model {request.Model.ToString().ToLower()}";
        }

        private int GetTimeoutMilliseconds(long fileSizeBytes, WhisperModels model)
        {
            long sizeInMB = fileSizeBytes / OneMegabyteInBytes;
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
            return Math.Clamp(estimatedMinutes, 1, 120) * OneMinuteAsMs;
        }

        private int GetDefaultTimeoutMilliseconds(WhisperModels model)
        {
            return model switch
            {
                WhisperModels.Tiny => 3 * OneMinuteAsMs,
                WhisperModels.Base => 6 * OneMinuteAsMs,
                WhisperModels.Small => 10 * OneMinuteAsMs,
                WhisperModels.Medium => 15 * OneMinuteAsMs,
                WhisperModels.Large => 25 * OneMinuteAsMs,
                _ => 6 * OneMinuteAsMs
            };
        }
    }
}