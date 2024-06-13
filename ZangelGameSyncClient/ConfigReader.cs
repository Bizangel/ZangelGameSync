namespace ZangelGameSyncClient
{
    internal class SyncConfigError(string message) : Exception(message) { }

    internal class ConfigReader
    {
        public struct GameSyncConfig
        {
            public string RemoteHost { get; init; }
            public string LocalSyncFolder { get; init; }
            public string RemoteFolderId { get; init; }
        }

        public static GameSyncConfig ReadConfig()
        {



            return new GameSyncConfig
            {
                LocalSyncFolder = "",
                RemoteFolderId = "",
                RemoteHost = "localhost",
            };
        }

        private static void ValidateConfig(GameSyncConfig config)
        {

            if (!Path.IsPathRooted(config.LocalSyncFolder))
                throw new SyncConfigError("Unable to specify");

            throw new NotImplementedException();
        }
    }
}
