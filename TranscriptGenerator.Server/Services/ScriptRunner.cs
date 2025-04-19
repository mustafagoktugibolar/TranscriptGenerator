using System.Diagnostics;
using System.Text;

namespace TranscriptGenerator.Server.Services
{
    public class ScriptRunner
    {
        public static async Task<(string output, string error, int exitCode)> RunPythonAsync(string arguments, int timeoutMs = 60000)
        {
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
                    Console.WriteLine("STDOUT: " + e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    Console.Error.WriteLine("STDERR: " + e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            var exited = await Task.Run(() => process.WaitForExit(timeoutMs));
            if (!exited)
            {
                process.Kill();
                return (string.Empty, "Timed out", -1);
            }

            return (outputBuilder.ToString(), errorBuilder.ToString(), process.ExitCode);
        }

    }
}

