using System.Net.Http.Json;
using TranscriptGenerator.Server.Models;
using TranscriptGenerator.Server.Services.Interfaces;
using TranscriptGenerator.Server.Helpers;
using System.Text.Json;

namespace TranscriptGenerator.Server.Services
{
    public class TranscriptService : ITranscriptService
    {
        private readonly HttpClient _httpClient;
        private readonly string[] _allowedExtensions = new[] { ".mp3", ".mp4", ".wav" };

        public TranscriptService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, string Result)> TranscribeYoutubeAsync(YoutubeTranscribeRequest request)
        {
            LogHelper.Info<TranscriptService>("Sending YouTube transcription request for URL: {Url}", request.Url);

            try
            {
                var response = await _httpClient.PostAsJsonAsync("/transcribe_youtube", new { url = request.Url, model = request.Model.ToString().ToLower() });
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("text", out var text))
                    {
                        return (true, text.GetString() ?? "");
                    }
                }
                
                var error = await response.Content.ReadAsStringAsync();
                LogHelper.Error<TranscriptService>(null, "Python service error: {Error}", error);
                return (false, $"Service error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                LogHelper.Error<TranscriptService>(ex, "Failed to communicate with Python service.");
                return (false, "Internal server error communicating with transcription service.");
            }
        }

        public async Task<(bool Success, string Result)> TranscribeFileAsync(TranscribeFileRequest request)
        {
            LogHelper.Info<TranscriptService>("Sending file transcription request for file: {FileName}", request.File.FileName);

            var file = request.File;
            string ext = Path.GetExtension(file.FileName).ToLower();

            if (!_allowedExtensions.Contains(ext))
            {
                return (false, "Invalid file type.");
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                
                content.Add(streamContent, "file", file.FileName);
                content.Add(new StringContent(request.Model.ToString().ToLower()), "model");

                var response = await _httpClient.PostAsync("/transcribe_file", content);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    if (result.TryGetProperty("text", out var text))
                    {
                        return (true, text.GetString() ?? "");
                    }
                }

                var error = await response.Content.ReadAsStringAsync();
                LogHelper.Error<TranscriptService>(null, "Python service error: {Error}", error);
                return (false, $"Service error: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                LogHelper.Error<TranscriptService>(ex, "Failed to communicate with Python service.");
                return (false, "Internal server error communicating with transcription service.");
            }
        }
    }
}