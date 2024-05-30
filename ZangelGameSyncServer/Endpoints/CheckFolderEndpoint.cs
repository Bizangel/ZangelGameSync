using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class CheckFolderEndpoint
    {
        public static async Task<String> Get(ILocalFolderService folderService, HttpRequest request)
        {
            return "Hello World! " + request.Query["folderId"];
        }
    }
}
