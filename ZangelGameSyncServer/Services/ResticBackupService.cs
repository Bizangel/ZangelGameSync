﻿using Microsoft.Extensions.Options;
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
                    FileName = Path.GetFullPath(_resticExecutablePath),
                    Arguments = args + $" -p {Path.GetFullPath(_resticPasswordFile)}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = _backupWorkingDirectory
                }
            };

            _logger.LogInformation("Executing: {FileName} with arguments {Arguments}", proc.StartInfo.FileName, proc.StartInfo.Arguments);

            proc.Start();

            Task<string> stdOut = proc.StandardOutput.ReadToEndAsync();
            Task<string> stdErr = proc.StandardError.ReadToEndAsync();

            await proc.WaitForExitAsync();

            string stdOutResult = await stdOut;
            string stdErrResult = await stdErr;

            _logger.LogDebug("Execution StdOut: {stdout}", stdOutResult);
            _logger.LogDebug("Execution StdErr: {stderr}", stdErrResult);

            return new ProcessResult
            {
                ExitCode = proc.ExitCode,
                StdOut = stdOutResult,
                StdErr = stdErrResult
            };
        }

        public async Task BackupFolder(string folderId)
        {
            throw new NotImplementedException();
            //logger.LogInformation("Creating Snapshot Backup for Folder: {folderId} ", folderId);


            //if (!BackupRepositoryExists(folderId))
            //{
            //    _logger.LogError("Attempting to backup {folderId}, without proper repository", folderId);
            //    throw new InvalidOperationException($"Attempting to backup folder {folderId} without proper repository");
            //}


            //logger.LogInformation($"Startign Backing up {folderId} folder");


            //// 1. Perform Backup
            //var procResult = await executeRestic($"-r {folderId} ");

            //// 2. Possible Forget/Prune older snapshots.

        }

        public bool BackupRepositoryExists(string folderId)
        {
            return _localFolderService.BackupFolderExists(folderId);
        }

        public async Task CreateBackupRepository(string folderId)
        {
            if (BackupRepositoryExists(folderId))
            {
                _logger.LogInformation($"Attempted Creation of existing Restic repository {folderId}, skipping creation");
                return;
            };

            _logger.LogInformation("Creating Restic Repository for {folderId}", folderId);
            var procResult = await executeRestic($"-r {folderId} init"); // cwd is backup folders so init properly

            if (procResult.ExitCode != 0)
            {
                _logger.LogError("Error when creating restic repository: {output}", procResult.StdOut + procResult.StdErr);
                throw new InvalidOperationException(procResult.StdErr + procResult.StdOut);
            }

            _logger.LogInformation("Successfully initialized Restic Repository for {folderId}", folderId);
            // succesfully initted repo
            return;
        }
    }
}
