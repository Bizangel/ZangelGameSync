using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class CreateFolderEndpoint
    {
        const string FolderIdQueryParameter = "folderId";
        public static async Task<IResult> Post(
            ILocalFolderService folderService, IBackupService backupService, HttpRequest request, ILogger<CheckFolderEndpoint> logger)
        {
            string? folderId = request.Query[FolderIdQueryParameter];
            if (folderId == null)
                return Results.BadRequest($"Missing {FolderIdQueryParameter}");

            if (!folderService.IsValidFolderId(folderId))
                return Results.BadRequest($"Invalid folder ID {folderId}");

            logger.LogInformation("Requested Creation of Folder: {folderId} ", folderId);

            try
            {
                // won't error out if folder already exists
                folderService.CreateSaveFolder(folderId);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to create folder: {folderId} {ex}", folderId, ex);
                return Results.Text($"Unable to create folder, see logs for more info. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }

            logger.LogInformation("Created Save Location Folder: {folderId} ", folderId);

            // won't error out if folder already exists
            try
            {
                await backupService.CreateBackupRepository(folderId);
            }
            catch (Exception ex)
            {
                logger.LogError("Failed to create backup folder: {folderId} {ex}", folderId, ex);
                return Results.Text($"Unable to create backup folder, see logs for more info. {ex.Message}", statusCode: StatusCodes.Status500InternalServerError);
            }

            logger.LogInformation("Created Save Backup Repository: {folderId} ", folderId);

            return Results.Ok("Successfully created folder!");
        }
    }
}
