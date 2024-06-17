using ZangelGameSyncClient.ConsoleLogging;
using ZangelGameSyncClient.Interfaces;

namespace ZangelGameSyncClient
{
    internal class SyncClientExceptionHandler
    {
        internal SyncClientExceptionHandler()
        {
        }

        internal async Task<ExitCode> Handle(Func<Task<ExitCode>> action)
        {
            try
            {
                return await action();
            }
            catch (Exception ex)
            {
                switch (ex)
                {
                    case SyncConfigException:
                        ConsolePrinter.Error($"Configuration Error\n{ex.Message}");
                        return ExitCode.CONFIG_ERROR;
                    case ArgumentException:
                        ConsolePrinter.Error($"Invalid Usage Error\n{ex.Message}");
                        return ExitCode.INVALID_USAGE;
                    case SyncServerUnreachableException:
                        ConsolePrinter.Error($"Connection Error to Server\n{ex.Message}");
                        return ExitCode.CONNECTION_ERROR;
                    case FileNotFoundException:
                        ConsolePrinter.Error(ex.Message);
                        return ExitCode.CONFIG_ERROR;
                    case SyncTransportException:
                        ConsolePrinter.Error(ex.Message);
                        return ExitCode.SYNC_ERROR;
                    default:
                        // unhandled exception, rethrow, but log to allow user to read error
                        Logger.LogError($"{ex.Message} Stack Trace\n: {ex.StackTrace}");
                        throw;
                }
            }
        }
    }
}
