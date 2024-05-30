namespace ZangelGameSyncServer.Interfaces
{
    public readonly struct LockAcquireResult
    {
        public string LockUser { get; init; }
        public bool AcquireSuccess { get; init; }
    }

    public enum LockReleaseResult
    {
        Forbidden,
        LockNotFound,
        Success
    }

    public interface IFolderLockService
    {
        public LockAcquireResult AcquireLock(string user, string folderName);
        public LockReleaseResult ReleaseLock(string user, string folderName);
    }
}
