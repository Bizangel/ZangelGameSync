using System.Text.Json;
using System.Text.RegularExpressions;

namespace ZangelGameSyncClient
{
    internal class SyncConfigException(string message) : Exception(message) { }
    public struct GameSyncConfig
    {
        public string RemoteUri { get; set; }
        public string LocalSyncFolder { get; set; }
        public string RemoteFolderId { get; init; }
        public string RemoteSaveFolder { get; set; }
        public string GameExecutable { get; set; }
    }

    internal partial class ConfigReader
    {
        [GeneratedRegex(@"^[A-Za-z][A-Za-z0-9_-]*$")]
        private static partial Regex FolderIdRegex();

        [GeneratedRegex("%(.*?)%")]
        private static partial Regex FolderEnvTemplateRegex();

        private string _configPath = String.Empty;

        public GameSyncConfig ReadConfig(string configPath)
        {
            _configPath = configPath;
            if (!File.Exists(configPath))
                throw new FileNotFoundException($"Unable to find file: {configPath}");

            var jsonString = File.ReadAllText(configPath);
            GameSyncConfig config = JsonSerializer.Deserialize<GameSyncConfig>(jsonString);

            config.LocalSyncFolder = EvaluatePathWithEnv(config.LocalSyncFolder);
            config.GameExecutable = EvaluatePathWithEnv(config.GameExecutable);
            config.RemoteSaveFolder = EvaluatePathWithEnv(config.RemoteSaveFolder);

            ValidateConfig(config);

            // Ensure that paths are "standard" i.e. using os-dependant separators, normalized etc.
            config.LocalSyncFolder = Path.GetFullPath(config.LocalSyncFolder);
            config.GameExecutable = Path.GetFullPath(config.GameExecutable);
            config.RemoteSaveFolder = Path.GetFullPath(config.RemoteSaveFolder);

            return config;
        }

        public static string EvaluatePathWithEnv(string pathWithEnv)
        {
            return FolderEnvTemplateRegex().Replace(pathWithEnv, match =>
            {
                string varName = match.Groups[1].Value;
                return Environment.GetEnvironmentVariable(varName) ?? match.Value;
            });
        }


        private void ValidateEvaluatedPath(string folderPath)
        {
            if (folderPath.Contains('\'') || folderPath.Contains('"') || folderPath.Contains('|'))
                throw new SyncConfigException($"Invalid path given: {folderPath}, found invalid characters. ");
        }

        private void ValidateConfig(GameSyncConfig config)
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

            if (String.IsNullOrEmpty(config.GameExecutable))
                throw new SyncConfigException("Missing configuration parameter GameExecutable. Please specify the path of the game executable to launch.");

            // Check that folder paths are well-formed, no weird character strings, or tricks to possibly maybe avoid injections (not really a big worry but eh being extra safe)
            ValidateEvaluatedPath(config.GameExecutable);
            ValidateEvaluatedPath(config.LocalSyncFolder);
            ValidateEvaluatedPath(config.RemoteSaveFolder);

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


            // Check game executable
            if (!Path.IsPathRooted(config.GameExecutable))
                throw new SyncConfigException($"Relative paths are not supported. Use absolute paths. {config.GameExecutable}");

            if (!File.Exists(config.GameExecutable))
                throw new SyncConfigException($"Game executable doesn't exist \"{config.GameExecutable}\"");

            // Check that config file is NOT writable. More of a security thing
            // Why?: This config is effectively being read and executing arbitrary commands based on this commands allowing for a very easy command injection. It's just an added security that definitely doesn't hurt.
            if (Utils.IsFileWritable(_configPath))
                throw new SyncConfigException("Configuration file IS writable! Please ensure write rules are present to avoid bad actors to execute other programs. Recommended write access is administrator only.");

            // Check that remote uri starts with either http / https
            if (!(config.RemoteUri.StartsWith("http://") || config.RemoteUri.StartsWith("https://")))
                throw new SyncConfigException($"Invalid RemoteUri ID: \"{config.RemoteUri}\" is invalid. Must must start with either http:// or https://");
        }
    }
}
