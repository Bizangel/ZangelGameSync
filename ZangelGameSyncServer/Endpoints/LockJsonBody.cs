using System.Text.Json;
using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Endpoints
{
    public class LockJsonBody
    {
        public string Hostname { get; set; } = String.Empty;
        public string FolderId { get; set; } = String.Empty;

        public static void Validate(LockJsonBody body, ILocalFolderService folderValidationService)
        {
            if (String.IsNullOrEmpty(body.Hostname) || String.IsNullOrEmpty(body.FolderId))
                throw new JsonException("Missing Json Properties: Hostname or FolderId");

            if (!folderValidationService.IsValidFolderId(body.Hostname))
                throw new JsonException($"Invalid Hostname: {body.Hostname} must conform to: ^[A-Za-z][A-Za-z0-9_-]*$");

            if (!folderValidationService.IsValidFolderId(body.FolderId))
                throw new JsonException($"Invalid FolderId: {body.FolderId} must conform to: ^[A-Za-z][A-Za-z0-9_-]*$");
        }

        public static async Task<LockJsonBody> Parse(HttpRequest request, ILocalFolderService folderValidationService)
        {
            LockJsonBody? body;
            body = await request.ReadFromJsonAsync<LockJsonBody>();
            if (body == null)
                throw new JsonException("Invalid JSON Body");

            Validate(body, folderValidationService);
            return body;
        }
    }
}
