using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZangelGameSyncClient
{
    internal class SyncConfigError(string message) : Exception(message) { }
    public struct GameSyncConfig
    {
        public string RemoteHost { get; init; }
        public string LocalSyncFolder { get; init; }
        public string RemoteFolderId { get; init; }
    }

    internal partial class ConfigReader
    {
        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_-]*$")]
        private static partial Regex FolderIdRegex();

        public static GameSyncConfig ReadConfig(string configPath)
        {

            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Unable to find file: {configPath}");

            var jsonString = File.ReadAllText(configPath);
            GameSyncConfig config = JsonSerializer.Deserialize<GameSyncConfig>(jsonString);

            ValidateConfig(config);
            return config;
        }


        private static void ValidateConfig(GameSyncConfig config)
        {
            // check defined
            if (String.IsNullOrEmpty(config.LocalSyncFolder))
                throw new SyncConfigError("Missing configuration parameter LocalSyncFolder. Please specify it (local folder to sync, should contain game saves)");

            if (String.IsNullOrEmpty(config.LocalSyncFolder))
                throw new SyncConfigError("Missing configuration parameter RemoteHost. Please specify a hostname like 192.168.0.10:1010 or example.com");

            if (String.IsNullOrEmpty(config.LocalSyncFolder))
                throw new SyncConfigError("Missing configuration parameter RemoteFolderId. Please specify the ID of the folder to associate with the local folder. This should be equal for all clients so that folder is synced properly.");

            // Check folder is valid
            if (!Path.IsPathRooted(config.LocalSyncFolder))
                throw new SyncConfigError($"Relative paths are not supported. Use absolute paths. {config.LocalSyncFolder}");

            if (!Path.Exists(config.LocalSyncFolder))
                throw new SyncConfigError($"Given sync folder not found, either create or specify the correct folder: {config.LocalSyncFolder}");

            // Check Remote folder ID is valid.
            if (!FolderIdRegex().IsMatch(config.RemoteFolderId))
                throw new SyncConfigError("Given folder ID is invalid. Must must conform to: ^[A-Za-z][A-Za-z0-9_-]*$");
        }
    }
}
