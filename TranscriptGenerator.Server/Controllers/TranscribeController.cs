using Microsoft.AspNetCore.Mvc;
using TranscriptGenerator.Server.Models;
using TranscriptGenerator.Server.Services.Interfaces;
using TranscriptGenerator.Server.Helpers;

namespace TranscriptGenerator.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TranscribeController : ControllerBase
    {
        private readonly ITranscriptService _transcriptService;

        public TranscribeController(ITranscriptService transcriptService)
        {
            _transcriptService = transcriptService;
        }

        [HttpPost("youtube")]
        public async Task<IActionResult> TranscribeYoutube([FromBody] YoutubeTranscribeRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
            {
                LogHelper.Warn<TranscribeController>("Empty YouTube URL received.");
                return BadRequest("URL cannot be empty.");
            }

            LogHelper.Info<TranscribeController>("Received YouTube transcription request: {Url}", request.Url);

            var (success, result) = await _transcriptService.TranscribeYoutubeAsync(request);
            return success ? Ok(new { transcript = result }) : StatusCode(500, result);
        }

        [HttpPost("file")]
        public async Task<IActionResult> TranscribeFile([FromForm] TranscribeFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
            {
                LogHelper.Warn<TranscribeController>("Empty or missing file in transcription request.");
                return BadRequest("File is required.");
            }

            LogHelper.Info<TranscribeController>("Received file transcription request: {FileName}, size: {Size} bytes", request.File.FileName, request.File.Length);

            var (success, result) = await _transcriptService.TranscribeFileAsync(request);
            return success ? Ok(new { transcript = result }) : StatusCode(500, result);
        }
    }
}