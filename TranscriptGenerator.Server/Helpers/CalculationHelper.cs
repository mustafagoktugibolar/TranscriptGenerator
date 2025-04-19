using TranscriptGenerator.Server.Shared.Enums;

namespace TranscriptGenerator.Server.Helpers
{
    public static class CalculationHelper
    {
        private const int OneMinuteAsMs = 60000;
        private const int OneMegabyteInBytes = 1048576;
        public static int GetTimeoutMsFromSize(long fileSizeBytes, WhisperModels model)
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

        public static int GetDefaultTimeoutMs(WhisperModels model)
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

        public static int GetTimeoutMsForMp3Download(WhisperModels model)
        {
            return GetDefaultTimeoutMs(model) * 2;
        }


    }
}
