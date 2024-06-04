namespace ZangelGameSyncClient
{
    internal class ConfigReader
    {

        public struct GameSyncConfig
        {
            public string remoteHost { get; init; }
            public string localSyncFolder { get; init; }
            public string remoteFolderId { get; init; }
        }

        public static GameSyncConfig ReadConfig()
        {
            throw new NotImplementedException();

            // validate config
            //ValidateConfig();
        }

        private static void ValidateConfig(GameSyncConfig config)
        {
            throw new NotImplementedException();
        }
    }
}
