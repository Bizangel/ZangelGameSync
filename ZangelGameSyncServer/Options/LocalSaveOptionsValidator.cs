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

            if (string.IsNullOrEmpty(options.ResticPasswordFile))
                return ValidateOptionsResult.Fail("Failed to specify \"ResticPasswordFile\" in appsettings.json");

            if (string.IsNullOrEmpty(options.ResticExecutablePath))
                return ValidateOptionsResult.Fail("Failed to specify \"ResticExecutablePath\" in appsettings.json");

            if (options.KeepSnapshotDays <= 1)
                return ValidateOptionsResult.Fail("KeepSnapshotDays must be greater than 1");

            if (!File.Exists(options.ResticPasswordFile))
                return ValidateOptionsResult.Fail($"Invalid ResticPasswordFile , file doesn't exist: {options.ResticPasswordFile}");

            if (!File.Exists(options.ResticExecutablePath))
                return ValidateOptionsResult.Fail($"Invalid ResticExecutablePath , file doesn't exist: {options.ResticExecutablePath}");

            if (String.IsNullOrEmpty(File.ReadAllText(options.ResticPasswordFile)))
                return ValidateOptionsResult.Fail($"Invalid ResticPasswordFile password file must NOT be empty!");

            if (!Directory.Exists(options.SaveFolderPath))
                return ValidateOptionsResult.Fail($"Invalid SaveFolderPath path doesn't exist: {options.SaveFolderPath}");
            if (!Directory.Exists(options.BackupFolderPath))
                return ValidateOptionsResult.Fail($"Invalid BackupFolderPath path given: {options.BackupFolderPath}");

            return ValidateOptionsResult.Success;
        }
    }
}

