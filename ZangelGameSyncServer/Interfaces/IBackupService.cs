namespace ZangelGameSyncServer.Interfaces
{
    public interface IBackupService
    {
        bool BackupRepositoryExists(string folderId);

        void CreateBackupRepository(string folderId);

        Task BackupFolder(string folderId);
    }
}
