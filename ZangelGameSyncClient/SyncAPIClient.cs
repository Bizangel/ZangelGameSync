using System.Net;

namespace ZangelGameSyncClient
{
    internal class SyncServerUnreachableException(string message) : Exception(message) { }
    internal class SyncFolderNotFoundException(string message, string folderId) : Exception(message)
    {
        internal string FolderId = folderId;
    }

    internal class SyncAPIClient
    {
        private readonly HttpClient client;

        internal SyncAPIClient(GameSyncConfig config)
        {
            client = new HttpClient
            {
                BaseAddress = new Uri(config.RemoteUri)
            };
            client.DefaultRequestHeaders.Accept.Clear(); // it doesn't really matter how it replies, as it's a custom API.
        }

        public async Task<long> CheckFolder(string folderId)
        {
            // Can Return Either 200, 404 (folderId doesn't exist), 400 bad request, or connection error
            try
            {
                ConsolePrinter.Info($"Checking folder: {folderId}...");
                var resp = await client.GetAsync($"/check-folder?folderId={folderId}");
                var text = await resp.Content.ReadAsStringAsync();

                if (resp.StatusCode == HttpStatusCode.NotFound)
                    throw new SyncFolderNotFoundException($"Sync folder with ID {folderId} not found.", folderId);

                if (resp.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Invalid application state, received status code: {resp.StatusCode} response: {text}");



                var resTimestamp = long.Parse(text);
                // TODO print here as human readable.
                ConsolePrinter.Info($"Fetched folder latest modified timestamp successfully: {resTimestamp}");
                return resTimestamp;
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }

        public async Task CreateFolder(string folderId)
        {
            // Can Return Either 200, 404 (folderId doesn't exist), 400 bad request, or connection error
            try
            {
                ConsolePrinter.Info($"Creating folder: {folderId}...");
                var resp = await client.GetAsync($"/create-folder?folderId={folderId}");
                var text = await resp.Content.ReadAsStringAsync();

                if (resp.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Unable to create folder, received status code: {resp.StatusCode} response: {text}");

                ConsolePrinter.Info($"Server response: {text}");
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }
    }
}
