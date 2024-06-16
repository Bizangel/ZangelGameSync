using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZangelGameSyncClient
{
    internal class SyncConfigException(string message) : Exception(message) { }
    public struct GameSyncConfig
    {
        public string RemoteUri { get; init; }
        public string LocalSyncFolder { get; init; }
        public string RemoteFolderId { get; init; }
        public string RemoteSaveFolder { get; init; }
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
                throw new SyncConfigException("Missing configuration parameter LocalSyncFolder. Please specify it (local folder to sync, should contain game saves)");

            if (String.IsNullOrEmpty(config.RemoteSaveFolder))
                throw new SyncConfigException("Missing configuration parameter RemoteSaveFolder. Please specify it (remote folder to sync with) for example: \\\\10.8.0.1\\saves or Z:\\saves. Please remember to escape \\ backslashes properly.");

            if (String.IsNullOrEmpty(config.RemoteUri))
                throw new SyncConfigException("Missing configuration parameter RemoteUri. Please specify a URI like http://192.168.0.10:1010 or https://example.com. You must specify either http or https");

            if (String.IsNullOrEmpty(config.RemoteFolderId))
                throw new SyncConfigException("Missing configuration parameter RemoteFolderId. Please specify the ID of the folder to associate with the local folder. This should be equal for all clients so that folder is synced properly.");

            // Check folder is valid
            if (!Path.IsPathRooted(config.LocalSyncFolder))
                throw new SyncConfigException($"Relative paths are not supported. Use absolute paths. {config.LocalSyncFolder}");

            if (!Path.Exists(config.LocalSyncFolder))
                throw new SyncConfigException($"Given sync folder not found, either create or specify the correct folder: {config.LocalSyncFolder}");

            // Check Remote folder ID is valid.
            if (!FolderIdRegex().IsMatch(config.RemoteFolderId))
                throw new SyncConfigException($"Given folder ID: \"{config.RemoteFolderId}\" is invalid. Must must conform to: ^[A-Za-z][A-Za-z0-9_-]*$");

            if (!Path.IsPathRooted(config.RemoteSaveFolder))
                throw new SyncConfigException($"Relative paths are not supported. Use absolute paths. {config.RemoteSaveFolder}");

            // Check that Remote Save folder exists.
            if (!Directory.Exists(config.RemoteSaveFolder))
                throw new SyncConfigException($"Remote save folder doesn't exist or is unreachable: \"{config.RemoteSaveFolder}\"");



            // Check that remote uri starts with either http / https
            if (!(config.RemoteUri.StartsWith("http://") || config.RemoteUri.StartsWith("https://")))
                throw new SyncConfigException($"Invalid RemoteUri ID: \"{config.RemoteUri}\" is invalid. Must must start with either http:// or https://");
        }
    }
}
