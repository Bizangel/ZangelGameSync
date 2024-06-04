using ZangelGameSyncClient;

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

// This command will preserve timestamp and sync from src to dst
// effectively working as a sort of rsync
// robocopy "C:\Users\bizan\Downloads\stellaris" "\\192.168.0.80\ZangelSaves\stellaris" /MIR /COPY:DAT / DCOPY:T

var client = new SyncAPIClient();

await client.checkFolder("stellaris");

//string saveFolderTest = @"C:\Users\bizan\Downloads\stellaris";

//long timestamp = new DateTimeOffset(
//    GetLatestModifiedTimestamp(saveFolderTest)
//).ToUnixTimeSeconds();

//Console.WriteLine($"Timestamp: {timestamp}");