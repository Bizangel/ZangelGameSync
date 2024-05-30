using Microsoft.Extensions.Options;
using ZangelGameSyncServer.Exceptions;
using ZangelGameSyncServer.Interfaces;
using ZangelGameSyncServer.Options;

namespace ZangelGameSyncServer.Services
{
    public class LocalFolderService : ILocalFolderService
    {
        private readonly LocalSaveOptions _saveOptions;

        public LocalFolderService(IOptions<LocalSaveOptions> options)
        {
            _saveOptions = options.Value;
        }

        public long GetFolderModifiedUnixTimestamp(string folderId)
        {
            string folderPath = _saveOptions.SaveFolderPath + "/" + folderId;
            if (!Directory.Exists(folderPath))
                throw new FolderNotFoundException($"Unable to find folder {folderPath}", folderId);

            long timestamp = new DateTimeOffset(
                Directory.GetLastWriteTimeUtc(folderPath)
            ).ToUnixTimeSeconds();

            return timestamp;
        }
    }
}
