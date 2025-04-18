using Microsoft.AspNetCore.Mvc;
using TranscriptGenerator.Server.Models;
using TranscriptGenerator.Server.Services.Interfaces;

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
                return BadRequest("URL cannot be empty.");

            var (success, result) = await _transcriptService.TranscribeYoutubeAsync(request);
            return success ? Ok(new { transcript = result }) : StatusCode(500, result);
        }

        [HttpPost("file")]
        public async Task<IActionResult> TranscribeFile([FromForm] TranscribeFileRequest request)
        {
            if (request.File == null || request.File.Length == 0)
                return BadRequest("File is required.");

            var (success, result) = await _transcriptService.TranscribeFileAsync(request);
            return success ? Ok(new { transcript = result }) : StatusCode(500, result);
        }
    }
}