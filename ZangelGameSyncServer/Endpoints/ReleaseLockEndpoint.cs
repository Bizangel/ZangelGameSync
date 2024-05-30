using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class ReleaseLockEndpoint
    {
        public static async Task<IResult> Post(
            IFolderLockService lockService,
            ILocalFolderService folderService,
            HttpRequest request,
            ILogger<ReleaseLockEndpoint> logger
        )
        {
            LockJsonBody jsonBody = await LockJsonBody.Parse(request, folderService);
            if (!folderService.SaveFolderExists(jsonBody.FolderId))
                return Results.NotFound($"Unable to find folder with ID: {jsonBody.FolderId}");

            logger.LogInformation("{Hostname} attempting to release lock {FolderId}", jsonBody.Hostname, jsonBody.FolderId);

            LockReleaseResult released = lockService.ReleaseLock(jsonBody.Hostname, jsonBody.FolderId);

            if (released == LockReleaseResult.LockNotFound)
                return Results.NotFound($"Unable to find lock associated with folder {jsonBody.FolderId}");

            if (released == LockReleaseResult.Forbidden)
                return Results.Text($"Unable to release lock as {jsonBody.Hostname}", statusCode: StatusCodes.Status403Forbidden);

            return Results.Ok($"Released lock for {jsonBody.FolderId} as {jsonBody.Hostname} successfully");
        }
    }
}
