using System.Globalization;

namespace ZangelGameSyncClient
{
    internal static class TimestampAPI
    {
        internal static DateTime GetLatestModifiedTimestamp(string folderPath, string excludePatternInput)
        {
            var excludedFiles = new HashSet<string>();
            string[] excludePatterns = excludePatternInput.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string pattern in excludePatterns)
            {
                foreach (string filePath in Directory.GetFiles(folderPath, pattern, SearchOption.AllDirectories))
                {
                    excludedFiles.Add(filePath);
                }
            }

            // Get latest timestamp recursively
            DateTime latestTimestamp = Directory.GetLastWriteTimeUtc(folderPath);
            // Get latest timestamp from files
            foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                if (excludedFiles.Contains(filePath)) continue;

                DateTime fileTimestamp = File.GetLastWriteTimeUtc(filePath);
                latestTimestamp = fileTimestamp > latestTimestamp ? fileTimestamp : latestTimestamp;
            }

            // get latest timestamp from folders
            foreach (string dirPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
            {
                DateTime dirTimestamp = Directory.GetLastWriteTimeUtc(dirPath);
                dirTimestamp = dirTimestamp > latestTimestamp ? dirTimestamp : latestTimestamp;
            }

            // return it
            return latestTimestamp;
        }
        internal static long GetLatestModifiedUnixTimestamp(string folderPath, string excludePattern)
        {
            return new DateTimeOffset(
                GetLatestModifiedTimestamp(folderPath, excludePattern)
            ).ToUnixTimeSeconds();
        }

        internal static string UnixTimestampToHumanReadableLocaleDependant(long unixTimestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;
            return localDateTime.ToString("F");
        }

        internal static string UnixTimestampToHumanReadable(long unixTimestamp)
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(unixTimestamp);
            DateTime localDateTime = dateTimeOffset.LocalDateTime;
            return localDateTime.ToString("F", new CultureInfo("en-US"));
        }
    }
}
