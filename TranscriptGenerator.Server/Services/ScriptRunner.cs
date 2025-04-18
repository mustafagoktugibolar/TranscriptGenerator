using System.Diagnostics;

namespace TranscriptGenerator.Server.Services
{
    public class ScriptRunner
    {
        public static async Task<(string output, string error, int exitCode)> RunPythonAsync(string arguments, int timeoutMilliseconds)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "python3",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = psi };
            process.Start();

            bool exited = process.WaitForExit(timeoutMilliseconds);
            string output = await process.StandardOutput.ReadToEndAsync();
            string error = await process.StandardError.ReadToEndAsync();

            if (!exited)
            {
                process.Kill();
                return (string.Empty, "Transcription timed out and was terminated.", -1);
            }

            return (output, error, process.ExitCode);
        }
    }
}

