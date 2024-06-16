using ZangelGameSyncClient;
using ZangelGameSyncClient.ConsoleLogging;
using ZangelGameSyncClient.SyncTransport;

// ===
// TODOs:
// - Implement remaining logic
// - Implement launch game logic
// - Add environment variable support to folders using %%. For example %APPDATA% is a common save folder path.
// - Consider some security measures and validation reading config (Folders should be scrutinized, no special chars, etc.)

ConsoleVisibilityAPI.AllocateConsole();

var exHandler = new SyncClientExceptionHandler();
bool lockAcquired = false;
var syncTransport = new RoboCopyTransport();

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

    // init transport
    await syncTransport.Init(config);

    // ===========================================
    // 2. Check for Folder timestamp. Server "may" be down.
    // ===========================================
    long remoteTimestamp = -1;
    try
    {
        try
        {
            remoteTimestamp = await apiClient.CheckFolder(config.RemoteFolderId);
        }
        catch (SyncFolderNotFoundException ex) // Folder might not exist, if so, create it seamlessly
        {
            ConsolePrinter.Warn($"Folder with ID {config.RemoteFolderId} not found! Creating the folder...");
            await apiClient.CreateFolder(ex.FolderId);
            remoteTimestamp = await apiClient.CheckFolder(config.RemoteFolderId);
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

    if (remoteTimestamp == -1)
        throw new Exception($"Invalid state, unable to fetch folder latest modified timestamp {config.RemoteFolderId}");

    // ===========================================
    // 3. Attempt to acquire lock.
    // ===========================================
    if (!await apiClient.AcquireLock(config.RemoteFolderId))
    {
        ConsolePrinter.Error("Folder sync is already being used!");
        bool confirmedContinue = ConsoleOptions.YesNoConfirm("Do you wish to continue playing? CLOUD SAVING WILL NOT WORK!",
            "\nAre you sure? There may be cloud sync conflict issues. And some data may be lost. To avoid this, sync as soon as possible when server is backup by re-launching the game. Are you still sure you want to proceed?", null);

        if (!confirmedContinue)
            return ExitCode.FAILED_LOCK;

        return ExitCode.SUCCESS; // skip all other logic just launch game
    }
    lockAcquired = true;

    // Compare obtained modified timestamp with local timestamp
    var localTimestamp = TimestampAPI.GetLatestModifiedUnixTimestamp(config.LocalSyncFolder);

    if (remoteTimestamp != localTimestamp)
    {
        ConsolePrinter.Warn($"[SYNC CONFLICT] Different Remote and Local timestamp found for folder: {config.RemoteFolderId}");
        int option = ConsoleOptions.ChoiceConfirm("Which version do you which to keep?",
            [$"Keep Remote ({TimestampAPI.UnixTimestampToHumanReadable(remoteTimestamp)})", $"Keep Local ({TimestampAPI.UnixTimestampToHumanReadable(localTimestamp)})"],
            ["Are you sure? All local data will be overwritten with the remote.", "Are you sure? All remote data will be overwritten with the local.", null]
        );


        if (option == 0) // Keep Remote
            await syncTransport.SyncPull(config.RemoteFolderId); // Override Local

        if (option == 1) // Keep Local
            await syncTransport.SyncPush(config.RemoteFolderId); // Override Remote

        ConsolePrinter.Success("Sync Successful!");
        return ExitCode.SUCCESS;
    }

    ConsolePrinter.Success("No Sync Conflict founds!");
    // No timestamp diff, just launch game perfectly 
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

ConsolePrinter.Info("Launching game...");
ConsoleVisibilityAPI.HideConsole();

Thread.Sleep(2_000);
ConsoleVisibilityAPI.ShowConsole();
Thread.Sleep(2_000);
/* 
 * =========== 
 * POST GAME EXECUTE LOGIC
 * =========== 
 */

// TODO implement remaining logic here

return 0;