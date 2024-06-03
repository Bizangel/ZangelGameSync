static DateTime GetLatestModifiedTimestamp(string folderPath)
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

string saveFolderTest = @"C:\Users\bizan\Downloads\stellaris";

long timestamp = new DateTimeOffset(
    GetLatestModifiedTimestamp(saveFolderTest)
).ToUnixTimeSeconds();

Console.WriteLine($"Timestamp: {timestamp}");