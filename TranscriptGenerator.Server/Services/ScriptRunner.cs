using System.Diagnostics;
using System.Text;
using TranscriptGenerator.Server.Helpers;

namespace TranscriptGenerator.Server.Services
{
    public class ScriptRunner
    {
        public static async Task<(string output, string error, int exitCode)> RunPythonAsync(string arguments, int timeoutMs = 60000)
        {
            LogHelper.Info<ScriptRunner>("Starting script with arguments: {Args}, timeout: {Timeout} ms", arguments, timeoutMs);
            var psi = new ProcessStartInfo
            {
                FileName = "py",
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var process = new Process { StartInfo = psi };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    LogHelper.Info<ScriptRunner>("STDOUT: {Line}", e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    LogHelper.Warn<ScriptRunner>("STDERR: {Line}", e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var exited = await Task.Run(() => process.WaitForExit(timeoutMs));
            if (!exited)
            {
                process.Kill();
                LogHelper.Warn<ScriptRunner>("Script timed out after {Timeout} ms. Args: {Args}", timeoutMs, arguments);
                return (string.Empty, "Timed out", -1);
            }

            int exitCode = process.ExitCode;
            LogHelper.Info<ScriptRunner>("Script finished with exit code {ExitCode}", exitCode);

            return (outputBuilder.ToString(), errorBuilder.ToString(), exitCode);
        }
    }
}