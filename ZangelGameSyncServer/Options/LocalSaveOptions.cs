namespace ZangelGameSyncServer.Options
{
    public class LocalSaveOptions
    {
        public const string SectionName = "LocalSave";

        public string BackupFolderPath { get; set; } = String.Empty;
        public string SaveFolderPath { get; set; } = String.Empty;
    }
}
