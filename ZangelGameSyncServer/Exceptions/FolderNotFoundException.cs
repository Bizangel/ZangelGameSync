namespace ZangelGameSyncServer.Exceptions
{
    public class FolderNotFoundException(string message, string folderId) : Exception(message)
    {
        public string FolderId = folderId;
    }
}
