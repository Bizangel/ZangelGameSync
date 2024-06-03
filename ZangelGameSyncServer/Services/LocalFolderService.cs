using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using ZangelGameSyncServer.Exceptions;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;

namespace ZangelGameSyncServer.Services
{
    public partial class LocalFolderService(IOptions<LocalSaveOptions> options) : ILocalFolderService
    {
        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_-]*$")]
        private static partial Regex FolderIdRegex();

        private readonly LocalSaveOptions _saveOptions = options.Value;
        private string BuildSaveFolderPath(string folderId) => _saveOptions.SaveFolderPath + "/" + folderId;
        private string BuildBackupFolderPath(string folderId) => _saveOptions.BackupFolderPath + "/" + folderId;

        private static DateTime GetLatestModifiedTimestamp(string folderPath)
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

        public long GetSaveFolderModifiedTimestamp(string folderId)
        {
            if (!SaveFolderExists(folderId))
                throw new FolderNotFoundException($"Unable to find folder {folderId}", folderId);

            long timestamp = new DateTimeOffset(
                GetLatestModifiedTimestamp(BuildSaveFolderPath(folderId))
            ).ToUnixTimeSeconds();

            return timestamp;
        }

        public void CreateSaveFolder(string folderId)
        {
            Directory.CreateDirectory(BuildSaveFolderPath(folderId));
        }

        public bool SaveFolderExists(string folderId)
        {
            return Directory.Exists(BuildSaveFolderPath(folderId));
        }

        public bool BackupFolderExists(string folderId)
        {
            return Directory.Exists(BuildBackupFolderPath(folderId));
        }

        public bool IsValidFolderId(string folderId)
        {
            return FolderIdRegex().IsMatch(folderId);
        }

        public string GetSaveFolderPath(string folderId) => BuildSaveFolderPath(folderId);
    }
}
