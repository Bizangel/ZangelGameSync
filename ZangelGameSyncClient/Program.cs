using System.Diagnostics;
using ZangelGameSyncClient;
using ZangelGameSyncClient.ConsoleLogging;
using ZangelGameSyncClient.SyncTransport;

// ===
// TODOs:
// - Add environment variable support to folders using %%. For example %APPDATA% is a common save folder path.
// - Consider some security measures and validation reading config (Folders should be scrutinized, no special chars, etc.)

ConsoleVisibilityAPI.AllocateConsole();

if (Utils.IsCurrentProcessElevated())
{
    ConsolePrinter.Error("Running as administrator is disallowed. Please take your security seriously. Run again.");
    ConsoleOptions.AwaitInput();
    return (int)ExitCode.INVALID_USAGE;
}

var exHandler = new SyncClientExceptionHandler();
bool lockAcquired = false;
var syncTransport = new RoboCopyTransport();
GameSyncConfig config = new GameSyncConfig();
SyncAPIClient? apiClient = null;

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

    config = new ConfigReader().ReadConfig(args[0]);
    apiClient = new SyncAPIClient(config);

    // init transport
    await syncTransport.Init(config);

    // ===========================================
    // 2. Check for Folder timestamp. Server "may" be down.
    // ===========================================
    long remoteTimestamp = -1;
    bool firstTimeCreated = false;
    try
    {
        try
        {
            remoteTimestamp = await apiClient.CheckFolder(config.RemoteFolderId);
        }
        catch (SyncFolderNotFoundException ex) // Folder might not exist, if so, create it seamlessly
        {
            firstTimeCreated = true;
            ConsolePrinter.Warn($"Folder with ID {config.RemoteFolderId} not found! Creating the folder...");
            await apiClient.CreateFolder(ex.FolderId);
            remoteTimestamp = await apiClient.CheckFolder(config.RemoteFolderId);
        }
    }
    catch (SyncServerUnreachableException ex) // Server is down. Give user option to continue or not.
    {
        ConsolePrinter.Error($"Error: {ex.Message}\nUnable to reach server CLOUD SAVING WILL NOT WORK!");
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

    // If first time created then just push don't compare.
    if (firstTimeCreated)
    {
        await syncTransport.SyncPush(config.RemoteFolderId);
        ConsolePrinter.Success("Sync Successful!");
        return ExitCode.SUCCESS;
    }


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

var proc = new Process
{
    StartInfo = new ProcessStartInfo
    {
        FileName = @"C:\Windows\System32\WindowsPowerShell\v1.0\powershell.exe", // powershell path
        Arguments = $"Start-Process -Wait {Path.GetFullPath(config.GameExecutable)}",
        UseShellExecute = false,
        WorkingDirectory = Directory.GetParent(config.GameExecutable)?.FullName
    }
};

ConsolePrinter.Info("Launching Game");
Logger.LogInfo($"Executing: {proc.StartInfo.FileName} with arguments {proc.StartInfo.Arguments}", print: false);

ConsoleVisibilityAPI.HideConsole();

proc.Start();
proc.WaitForExit();

/* 
 * =========== 
 * POST GAME EXECUTE LOGIC
 * =========== 
 */

if (!lockAcquired) // nothing to do if no lock
    return 0;

// show console after game exit
ConsoleVisibilityAPI.ShowConsole();

var postExitCode = (int)await exHandler.Handle(async () =>
{
    if (apiClient == null)
        throw new Exception("Invalid client state.");

    // Check for server being up.
    bool serverUp = false;
    while (!serverUp)
    {
        try
        {
            await apiClient.CheckFolder(config.RemoteFolderId);
            serverUp = true;
        }
        catch (SyncServerUnreachableException ex)
        {
            ConsoleOptions.YesNoConfirm("Sync server is unreachable or down. Do you want to retry reaching it?", null, "Are you sure? your save progress won't be synced.");
        }
    }

    ConsolePrinter.Success("Server is up, performing sync...");

    // server is now up, perform sync
    await syncTransport.SyncPush(config.RemoteFolderId);
    ConsolePrinter.Success("Successfully synced game save");

    // sync performed -> now release lock
    await apiClient.ReleaseLock(config.RemoteFolderId);
    ConsolePrinter.Info("Released lock successfully");

    // Now trigger backup on host
    await apiClient.CreateBackupSnapshot(config.RemoteFolderId);
    ConsolePrinter.Success("Successfully backuped game save on Sync server!");

    // if fully successful wait a split-sec so we can at least glance at green
    Thread.Sleep(1500);
    return ExitCode.SUCCESS;
});

if (postExitCode != 0)
    ConsoleOptions.AwaitInput(); // if non zero wait so errors can be read etc.

return postExitCode;