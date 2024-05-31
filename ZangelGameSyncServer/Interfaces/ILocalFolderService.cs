namespace ZangelGameSyncServer.Interfaces
{
    public interface ILocalFolderService
    {
        public long GetSaveFolderModifiedTimestamp(string folderId);
        public bool SaveFolderExists(string folderId);
        public bool BackupFolderExists(string folderId);
        public bool IsValidFolderId(string folderId);
        public string GetBackupFolderPath(string folderId);
        public void CreateSaveFolder(string folderId);
    };
}
