namespace ZangelGameSyncServer.Options
{
    public class LocalSaveOptions
    {
        public const string SECTION_NAME = "LocalSave";

        public string BackupFolderPath { get; set; } = String.Empty;
        public string SaveFolderPath { get; set; } = String.Empty;
    }
}
