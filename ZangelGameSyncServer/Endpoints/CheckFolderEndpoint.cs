using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class CheckFolderEndpoint
    {
        const string FolderIdQueryParameter = "folderId";

        public static IResult Get(ILocalFolderService folderService, HttpRequest request, ILogger<CheckFolderEndpoint> logger)
        {
            string? folderId = request.Query[FolderIdQueryParameter];
            if (folderId == null)
                return Results.BadRequest($"Missing {FolderIdQueryParameter}");

            if (!folderService.IsValidFolderId(folderId))
                return Results.BadRequest($"Invalid folder ID {folderId}");

            logger.LogInformation($"Timestamp for folder ID: {folderId} requested");

            long timeStamp = folderService.GetSaveFolderModifiedTimestamp(folderId);
            return Results.Ok(timeStamp);
        }
    }
}
