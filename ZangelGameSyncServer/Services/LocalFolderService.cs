using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using ZangelGameSyncServer.Exceptions;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;

namespace ZangelGameSyncServer.Services
{
    public partial class LocalFolderService : ILocalFolderService
    {
        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_-]*$")]
        private static partial Regex FolderIdRegex();

        private readonly LocalSaveOptions _saveOptions;
        private string BuildFolderPath(string folderId) => _saveOptions.SaveFolderPath + "/" + folderId;

        public LocalFolderService(IOptions<LocalSaveOptions> options)
        {
            _saveOptions = options.Value;
        }

        public long GetFolderModifiedUnixTimestamp(string folderId)
        {
            if (SaveFolderExists(folderId))
                throw new FolderNotFoundException($"Unable to find folder {folderId}", folderId);

            long timestamp = new DateTimeOffset(
                Directory.GetLastWriteTimeUtc(BuildFolderPath(folderId))
            ).ToUnixTimeSeconds();

            return timestamp;
        }

        public bool SaveFolderExists(string folderId)
        {
            return Directory.Exists(BuildFolderPath(folderId));
        }

        public bool IsValidFolderId(string folderId)
        {
            return FolderIdRegex().IsMatch(folderId);
        }
    }
}
