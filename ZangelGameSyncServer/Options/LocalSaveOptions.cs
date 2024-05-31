namespace ZangelGameSyncServer.Options
{
    public class LocalSaveOptions
    {
        public const string SectionName = "LocalSave";

        public string BackupFolderPath { get; set; } = String.Empty;
        public string SaveFolderPath { get; set; } = String.Empty;
        public int KeepSnapshotDays { get; set; } = -1;
        public int MaxBackupSnapshots { get; set; } = -1;
        public string ResticPasswordFile { get; set; } = String.Empty;
        public string ResticExecutablePath { get; set; } = String.Empty;
    }
}
