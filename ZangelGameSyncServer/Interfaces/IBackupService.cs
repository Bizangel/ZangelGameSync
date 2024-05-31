namespace ZangelGameSyncServer.Interfaces
{
    public interface IBackupService
    {
        bool BackupRepositoryExists(string folderId);

        Task CreateBackupRepository(string folderId);

        Task BackupFolder(string folderId);
    }
}
