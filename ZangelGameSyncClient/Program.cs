using ZangelGameSyncClient;

//static DateTime GetLatestModifiedTimestamp(string folderPath)
//{
//    // Get latest timestamp recursively
//    DateTime latestTimestamp = Directory.GetLastWriteTimeUtc(folderPath);
//    // Get latest timestamp from files
//    foreach (string filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
//    {
//        DateTime fileTimestamp = File.GetLastWriteTimeUtc(filePath);
//        latestTimestamp = fileTimestamp > latestTimestamp ? fileTimestamp : latestTimestamp;
//    }

//    // get latest timestamp from folders
//    foreach (string dirPath in Directory.GetDirectories(folderPath, "*", SearchOption.AllDirectories))
//    {
//        DateTime dirTimestamp = Directory.GetLastWriteTimeUtc(dirPath);
//        dirTimestamp = dirTimestamp > latestTimestamp ? dirTimestamp : latestTimestamp;
//    }

//    // return it
//    return latestTimestamp;
//}

// ===
// TODOs:
// - Implement remaining logic
// - Implement launch game logic
// - Add environment variable support to folders using %%. For example %APPDATA% is a common save folder path.
// - Consider some security measures and validation reading config (Folders should be scrutinized, no special chars, etc.)
// - Add logic to get LOCAL folder modified timestamp recursively.
// - Add logic to print timestamp in human readable format. Ideally display timezone to avoid issues. But should use local PC timezone.
// - Implement actual "copying" logic. Use ROBOCOPY for now.

var exHandler = new SyncClientExceptionHandler();
bool lockAcquired = false;

/* 
 * =========== 
 * PRE-GAME EXECUTE LOGIC
 * =========== 
 */
var preExecutionExitCode = (int)await exHandler.Handle(async () =>
{
    // ===========================================
    // 1. Check for config.
    // ===========================================
    if (args.Length != 1)
        throw new ArgumentException("Invalid usage. Specify config file path as first argument");

    var config = ConfigReader.ReadConfig(args[0]);
    var apiClient = new SyncAPIClient(config);

    // ===========================================
    // 2. Check for Folder timestamp. Server "may" be down.
    // ===========================================
    long result = -1;
    try
    {
        try
        {
            result = await apiClient.CheckFolder(config.RemoteFolderId);
        }
        catch (SyncFolderNotFoundException ex) // Folder might not exist, if so, create it seamlessly
        {
            ConsolePrinter.Warn($"Folder with ID {config.RemoteFolderId} not found! Creating the folder...");
            await apiClient.CreateFolder(ex.FolderId);
            result = await apiClient.CheckFolder(config.RemoteFolderId);
        }
    }
    catch (SyncServerUnreachableException) // Server is down. Give user option to continue or not.
    {
        ConsolePrinter.Error("Unable to reach server CLOUD SAVING WILL NOT WORK!");
        bool confirmedContinue = ConsoleOptions.YesNoConfirm("Do you wish to continue playing? CLOUD SAVING WILL NOT WORK!",
            "\nAre you sure? There may be cloud sync conflict issues. And some data may be lost. To avoid this, sync as soon as possible when server is backup by re-launching the game. Are you still sure you want to proceed?", null);

        if (!confirmedContinue)
            return ExitCode.CONNECTION_ERROR;

        return ExitCode.SUCCESS; // skip all other logic, just launch game.
    }

    if (result == -1)
        throw new Exception($"Invalid state, unable to fetch folder latest modified timestamp {config.RemoteFolderId}");

    // ===========================================
    // 3. Attempt to acquire lock.
    // ===========================================

    return ExitCode.SUCCESS;
});

if (preExecutionExitCode != 0)
{
    ConsoleOptions.AwaitInput();
    return preExecutionExitCode; // stop if non Successful
}

/* 
 * =========== 
 * GAME EXECUTE LOGIC
 * =========== 
 */

// TODO launch game here

/* 
 * =========== 
 * POST GAME EXECUTE LOGIC
 * =========== 
 */

// TODO implement remaining logic here

return 0;