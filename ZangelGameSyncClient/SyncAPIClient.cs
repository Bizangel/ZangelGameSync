using System.Net;
using System.Text;
using System.Text.Json;
using ZangelGameSyncClient.ConsoleLogging;

namespace ZangelGameSyncClient
{
    internal class SyncServerUnreachableException(string message) : Exception(message) { }

    internal class SyncFolderNotFoundException(string message, string folderId) : Exception(message)
    {
        internal string FolderId = folderId;
    }

    internal class LockJsonBody
    {
        public string Hostname { get; set; } = String.Empty;
        public string FolderId { get; set; } = String.Empty;

        public StringContent AsJsonBody()
        {
            var text = JsonSerializer.Serialize(this);
            return new StringContent(text, Encoding.UTF8, "application/json");
        }
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
                ConsolePrinter.Info($"Fetched folder latest modified timestamp successfully: {TimestampAPI.UnixTimestampToHumanReadable(resTimestamp)}");
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
                var resp = await client.PostAsync($"/create-folder?folderId={folderId}", null);

                var text = await resp.Content.ReadAsStringAsync();

                if (resp.StatusCode != HttpStatusCode.OK)
                    throw new Exception($"Unable to create folder {folderId}, received status code: {(int)resp.StatusCode} response: {text}\n");

                ConsolePrinter.Info($"Server response: {text}");
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }


        public async Task<bool> AcquireLock(string folderId)
        {
            // Can Return Either 200, 403 (someone else using lock), 404 requesting lock for invalid folder, 400 bad request, or connection error
            try
            {
                ConsolePrinter.Info($"Acquiring lock for folder: {folderId}...");
                var resp = await client.PostAsync($"/acquire-folder-lock",
                    new LockJsonBody
                    {
                        FolderId = folderId,
                        Hostname = Environment.MachineName
                    }
                    .AsJsonBody()
                );

                var text = await resp.Content.ReadAsStringAsync();

                switch (resp.StatusCode)
                {
                    case HttpStatusCode.OK:
                        ConsolePrinter.Info($"Server response: {text}");
                        return true; // acquired lock
                    case HttpStatusCode.Forbidden: // already being used
                        ConsolePrinter.Error(text);
                        return false; // unable to acquire lock
                    default:
                        throw new Exception($"Unable to acquire lock on folder {folderId}, received status code: {(int)resp.StatusCode} response: {text}\n");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }

        public async Task ReleaseLock(string folderId)
        {
            // Can Return Either 200, 403 (someone else's lock), releasing lock for invalid folder / already released, 400 bad request, or connection error
            try
            {
                ConsolePrinter.Info($"Releasing lock for folder: {folderId}...");
                var resp = await client.PostAsync($"/release-folder-lock",
                    new LockJsonBody
                    {
                        FolderId = folderId,
                        Hostname = Environment.MachineName
                    }
                    .AsJsonBody()
                );

                var text = await resp.Content.ReadAsStringAsync();

                switch (resp.StatusCode)
                {
                    case HttpStatusCode.OK:
                        ConsolePrinter.Info($"Server response: {text}");
                        return;
                    default:
                        throw new Exception($"Unable to release lock on folder {folderId}, received status code: {(int)resp.StatusCode} response: {text}\n");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }

        public async Task CreateBackupSnapshot(string folderId)
        {
            // Can Return Either 200, 404 folder doesn't exist, 400 bad request, or connection error
            try
            {
                ConsolePrinter.Info($"Requesting backup for folder: {folderId}...");
                var resp = await client.PostAsync($"/backup-snapshot?folderId={folderId}", null);
                var text = await resp.Content.ReadAsStringAsync();

                switch (resp.StatusCode)
                {
                    case HttpStatusCode.OK:
                        ConsolePrinter.Info($"Server response: {text}");
                        return;
                    default:
                        throw new Exception($"Unable to backup {folderId} folder, received status code: {(int)resp.StatusCode} response: {text}\n");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new SyncServerUnreachableException(ex.Message);
            }
        }
    }
}
