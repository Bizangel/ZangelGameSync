using ZangelGameSyncServer.Interfaces;

namespace ZangelGameSyncServer.Services
{
    public class FolderLockService(ILogger<FolderLockService> logger) : IFolderLockService
    {
        public readonly Dictionary<string, string> _folderLocks = [];
        private readonly object _dictLock = new();
        private readonly ILogger _logger = logger;

        public LockAcquireResult AcquireLock(string requestingUser, string folderName)
        {
            lock (_dictLock)
            {
                _folderLocks.TryGetValue(folderName, out string? lockUser);
                // no one has acquired folder, can acquire
                if (lockUser == null)
                {
                    _logger.LogInformation("{User} Acquired Lock on {Folder}", requestingUser, folderName);
                    _folderLocks.Add(folderName, requestingUser);
                    return new LockAcquireResult()
                    {
                        LockUser = requestingUser,
                        AcquireSuccess = true
                    };
                }

                // Else failed, return user using lock
                return new LockAcquireResult()
                {
                    LockUser = lockUser,
                    AcquireSuccess = false
                };
            }
        }

        public LockReleaseResult ReleaseLock(string requestingUser, string folderName)
        {
            lock (_dictLock)
            {
                _folderLocks.TryGetValue(folderName, out string? lockUser);
                if (lockUser == null)
                    return LockReleaseResult.LockNotFound;

                if (lockUser == requestingUser)
                {
                    _logger.LogInformation("{User} Released lock on {Folder}", requestingUser, folderName);
                    _folderLocks.Remove(folderName);
                    return LockReleaseResult.Success;
                }

                // Cannot release another user's lock
                return LockReleaseResult.Forbidden;
            }
        }
    }
}
