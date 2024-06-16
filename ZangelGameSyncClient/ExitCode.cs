namespace ZangelGameSyncClient
{
    internal enum ExitCode
    {
        SUCCESS = 0,
        INVALID_USAGE = 1,
        CONFIG_ERROR = 2,
        CONNECTION_ERROR = 3,
        FAILED_LOCK = 4,
        CONFLICT_STOP = 5,
    }
}
