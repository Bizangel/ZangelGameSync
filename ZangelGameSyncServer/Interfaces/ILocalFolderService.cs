using ZangelGameSyncServer.Options;

namespace ZangelGameSyncServer.Interfaces
{
    public interface ILocalFolderService
    {
        public long GetFolderModifiedUnixTimestamp(string folder);
    }
}
