namespace ZangelGameSyncServer.Interfaces
{
    public interface ILocalFolderService
    {
        public long GetFolderModifiedUnixTimestamp(string folderId);
        public bool SaveFolderExists(string folderId);
        public bool IsValidFolderId(string folderId);
    }
}
