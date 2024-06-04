namespace ZangelGameSyncClient
{
    internal class SyncAPIClient
    {
        static HttpClient client = new HttpClient();

        internal SyncAPIClient()
        {
            client.BaseAddress = new Uri("http://192.168.0.80:7000");
            client.DefaultRequestHeaders.Accept.Clear(); // it doesn't really matter how it replies, as it's a custom API.
        }

        public async Task<long> checkFolder(string folderId)
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
