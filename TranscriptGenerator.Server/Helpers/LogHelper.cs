namespace TranscriptGenerator.Server.Helpers
{
    public static class LogHelper
    {
        private static ILoggerFactory? _loggerFactory;

        public static void Configure(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public static ILogger CreateLogger<T>() =>
            _loggerFactory?.CreateLogger<T>() ?? throw new InvalidOperationException("LoggerFactory not configured");

        public static void Info<T>(string message, params object[] args) =>
            CreateLogger<T>().LogInformation(message, args);

        public static void Error<T>(Exception ex, string message, params object[] args) =>
            CreateLogger<T>().LogError(ex, message, args);

        public static void Warn<T>(string message, params object[] args) =>
            CreateLogger<T>().LogWarning(message, args);
    }
}
