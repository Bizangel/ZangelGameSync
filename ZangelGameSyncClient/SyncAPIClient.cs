namespace ZangelGameSyncClient
{
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
            try
            {
                ConsolePrinter.Info($"Checking folder: {folderId}...");
                var resp = await client.GetAsync($"/check-folder?folderId={folderId}");
                var text = await resp.Content.ReadAsStringAsync();

                return long.Parse(text);
            }
            catch (HttpRequestException ex)
            {
                ConsolePrinter.Error("Connection Error: ");
                ConsolePrinter.Error(ex.Message);
                return -1;
            }
        }
    }
}
