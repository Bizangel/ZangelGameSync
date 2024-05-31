using Microsoft.Extensions.Options;
using System.Diagnostics;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;

namespace ZangelGameSyncServer.Services
{
    public readonly struct ProcessResult
    {
        public readonly int ExitCode { get; init; }
        public readonly string StdOut { get; init; }
        public readonly string StdErr { get; init; }
    }

    public class ResticBackupService(
        IOptions<LocalSaveOptions> saveOptions,
        ILocalFolderService localFolderService,
        ILogger<ResticBackupService> logger
        ) : IBackupService
    {
        private readonly int _snapshotKeepDays = saveOptions.Value.KeepSnapshotDays;
        private readonly ILocalFolderService _localFolderService = localFolderService;
        private readonly string _resticPasswordFile = saveOptions.Value.ResticPasswordFile;
        private readonly string _resticExecutablePath = saveOptions.Value.ResticExecutablePath;
        private readonly string _backupWorkingDirectory = saveOptions.Value.BackupFolderPath;
        private readonly ILogger _logger = logger;

        private async Task<ProcessResult> executeRestic(string args)
        {
            var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.GetFullPath(_resticPasswordFile),
                    Arguments = args + $"-p ${Path.GetFullPath(_resticPasswordFile)}",
                    UseShellExecute = false,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                    CreateNoWindow = true,
                    WorkingDirectory = _backupWorkingDirectory
                }
            };

            _logger.LogInformation("Executing: {FileName} with arguments {Arguments}", proc.StartInfo.FileName, proc.StartInfo.Arguments);

            proc.Start();
            Task<string> stdOut = proc.StandardOutput.ReadToEndAsync();
            Task<string> stdErr = proc.StandardError.ReadToEndAsync();

            await proc.WaitForExitAsync();

            return new ProcessResult
            {
                ExitCode = proc.ExitCode,
                StdOut = await stdOut,
                StdErr = await stdErr
            };
        }

        public async Task BackupFolder(string folderId)
        {
            throw new NotImplementedException();

            if (!BackupRepositoryExists(folderId))
            {
                _logger.LogError("Attempting to backup {folderId}, without proper repository", folderId);
                throw new InvalidOperationException($"Attempting to backup folder {folderId} without proper repository");
            }


            logger.LogInformation($"Startign Backing up {folderId} folder");


            // 1. Perform Backup
            var procResult = await executeRestic($"-r {folderId} ");

            // 2. Possible Forget/Prune older snapshots.

        }

        public bool BackupRepositoryExists(string folderId)
        {
            return _localFolderService.BackupFolderExists(folderId);
        }

        public void CreateBackupRepository(string folderId)
        {
            throw new NotImplementedException();
        }
    }
}
