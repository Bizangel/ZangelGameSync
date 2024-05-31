using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class BackupSnapshotEndpoint
    {
        const string FolderIdQueryParameter = "folderId";
        public static async Task<IResult> Post(
            ILocalFolderService folderService, HttpRequest request, ILogger<CheckFolderEndpoint> logger, IBackupService backupService)
        {
            string? folderId = request.Query[FolderIdQueryParameter];
            if (folderId == null)
                return Results.BadRequest($"Missing {FolderIdQueryParameter}");

            if (!folderService.IsValidFolderId(folderId))
                return Results.BadRequest($"Invalid folder ID {folderId}");

            logger.LogInformation("Taking Backup Snapshot for {folderId} folder", folderId);

            await backupService.BackupFolder(folderId);

            logger.LogInformation("Backup Snapshot Successfully taken for {folderId} at {humanTimestamp}", folderId, DateTime.Now.ToString("[yyyy-MM-dd HH:mm:ss]"));
            return Results.Ok($"Backup Taken Successfully for {folderId}");
        }
    }
}
