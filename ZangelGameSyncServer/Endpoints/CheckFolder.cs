using Microsoft.AspNetCore.SignalR;
using System.Dynamic;

namespace ZangelGameSyncServer.Endpoints
{
    public class CheckFolderEndpoint
    {
        public static async Task<String> Get(HttpRequest request)
        {
            return "Hello World! " + request.Query["folderId"];
        }
    }
}
