using System.Globalization;

namespace ZangelGameSyncClient
{
    internal static class TimestampAPI
    {
        internal static DateTime GetLatestModifiedTimestamp(string folderPath)
        {
            // Get latest timestamp recursively
            DateTime latestTimestamp = Directory.GetLastWriteTimeUtc(folderPath);
            // Get latest timestamp from files
            foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
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
        internal static long GetLatestModifiedUnixTimestamp(string folderPath)
        {
            return new DateTimeOffset(
                GetLatestModifiedTimestamp(folderPath)
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
