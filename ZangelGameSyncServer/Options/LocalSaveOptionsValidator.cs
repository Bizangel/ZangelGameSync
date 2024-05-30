using Microsoft.Extensions.Options;

namespace ZangelGameSyncServer.Options
{
    public class LocalSaveOptionsValidator : IValidateOptions<LocalSaveOptions>
    {
        public ValidateOptionsResult Validate(string? name, LocalSaveOptions options)
        {
            if (string.IsNullOrEmpty(options.SaveFolderPath))
                return ValidateOptionsResult.Fail("Failed to specify \"SaveFolderPath\" in appsettings.json");

            if (string.IsNullOrEmpty(options.BackupFolderPath))
                return ValidateOptionsResult.Fail("Failed to specify \"BackupFolderPath\" in appsettings.json");

            if (!Directory.Exists(options.SaveFolderPath))
                return ValidateOptionsResult.Fail($"Invalid SaveFolderPath path doesn't exist: {options.SaveFolderPath}");
            if (!Directory.Exists(options.BackupFolderPath))
                return ValidateOptionsResult.Fail($"Invalid BackupFolderPath path given: {options.BackupFolderPath}");

            return ValidateOptionsResult.Success;
        }
    }
}

