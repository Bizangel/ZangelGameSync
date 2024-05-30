using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class AcquireLockEndpoint
    {
        public static async Task<IResult> Post(
            IFolderLockService lockService,
            ILocalFolderService folderService,
            HttpRequest request,
            ILogger<AcquireLockEndpoint> logger
        )
        {
            LockJsonBody jsonBody = await LockJsonBody.Parse(request, folderService);
            if (!folderService.SaveFolderExists(jsonBody.FolderId))
                return Results.NotFound($"Unable to find folder with ID: {jsonBody.FolderId}");

            logger.LogInformation("{Hostname} attempting to acquire lock {FolderId}", jsonBody.Hostname, jsonBody.FolderId);

            LockAcquireResult acquired = lockService.AcquireLock(jsonBody.Hostname, jsonBody.FolderId);

            if (!acquired.AcquireSuccess)
                return Results.Text($"{acquired.LockUser} is already using {jsonBody.FolderId} folder", statusCode: StatusCodes.Status403Forbidden);

            return Results.Ok($"Acquired lock for {jsonBody.FolderId} as {jsonBody.Hostname} successfully");
        }
    }
}
