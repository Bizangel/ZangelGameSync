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

        public long GetSaveFolderModifiedTimestamp(string folderId)
        {
            if (SaveFolderExists(folderId))
                throw new FolderNotFoundException($"Unable to find folder {folderId}", folderId);

            long timestamp = new DateTimeOffset(
                Directory.GetLastWriteTimeUtc(BuildSaveFolderPath(folderId))
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

        public string GetBackupFolderPath(string folderId) => BuildBackupFolderPath(folderId);
    }
}
