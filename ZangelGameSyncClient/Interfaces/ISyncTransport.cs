namespace ZangelGameSyncClient.Interfaces
{
    internal interface ISyncTransport
    {
        Task Init(GameSyncConfig config);
        Task SyncPush(string folderId);
        Task SyncPull(string folderId);
    }
}
