using System.Diagnostics;
using ZangelGameSyncClient.ConsoleLogging;
using ZangelGameSyncClient.Interfaces;

namespace ZangelGameSyncClient.SyncTransport
{
    internal class RoboCopyTransport : ISyncTransport
    {
        private const string ROBOCOPY_PATH = @"C:\Windows\System32\Robocopy.exe";
        private static void EnsureRobocopyExists()
        {
            if (!File.Exists(ROBOCOPY_PATH))
            {
                throw new Exception($"Unable to find Robocopy.exe at {ROBOCOPY_PATH} Stopping.");
            }
        }

        private GameSyncConfig _config;

        private void ExecuteRobocopy(string srcPath, string dstPath)
        {

            var excludeFlag = String.IsNullOrEmpty(_config.SyncExclude) ? "" : $" /XF {_config.SyncExclude}";
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = ROBOCOPY_PATH,
                    Arguments = $"\"{srcPath}\" \"{dstPath}\" /MIR /COPY:DAT /DCOPY:T" + excludeFlag, // flags to keep timestamp, make copy and delete if needed.
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                }
            };

            Logger.LogInfo($"Executing: {proc.StartInfo.FileName} with arguments {proc.StartInfo.Arguments}");

            proc.Start();

            string stdOut = proc.StandardOutput.ReadToEnd();
            string stdErr = proc.StandardError.ReadToEnd();

            proc.WaitForExit();

            Logger.LogInfo(stdOut);

            if (!String.IsNullOrWhiteSpace(stdErr))
                Logger.LogError(stdErr);

            if (proc.ExitCode >= 8) // 8 or higher is robocopy error see: https://learn.microsoft.com/en-us/windows-server/administration/windows-commands/robocopy
            {
                throw new SyncTransportException($"Robocopy ERROR exit code: {proc.ExitCode}. Stopping.");
            }
        }

        private string GetLocalFolderPath() => Path.GetFullPath(_config.LocalSyncFolder);
        private string BuildRemoteFolderPath(string folderId) => Path.Combine(Path.GetFullPath(_config.RemoteSaveFolder), folderId);

        public Task Init(GameSyncConfig config)
        {
            _config = config;
            EnsureRobocopyExists();
            return Task.CompletedTask;
        }

        public Task SyncPull(string folderId)
        {
            ExecuteRobocopy(BuildRemoteFolderPath(folderId), GetLocalFolderPath()); // override local
            return Task.CompletedTask;
        }

        public Task SyncPush(string folderId)
        {
            ExecuteRobocopy(GetLocalFolderPath(), BuildRemoteFolderPath(folderId)); // override remote
            return Task.CompletedTask;
        }
    }
}
