namespace ZangelGameSyncClient.Interfaces
{
    internal class SyncTransportException(string message) : Exception(message) { }
    internal interface ISyncTransport
    {
        Task Init(GameSyncConfig config);
        Task SyncPush(string folderId);
        Task SyncPull(string folderId);
    }
}
