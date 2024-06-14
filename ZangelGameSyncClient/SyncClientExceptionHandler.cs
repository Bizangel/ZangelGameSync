namespace ZangelGameSyncClient
{
    public class SyncClientExceptionHandler
    {
        public SyncClientExceptionHandler()
        {
        }

        public async Task Handle(Func<Task> action)
        {
            try
            {
                await action();
            }
            catch (Exception ex) {
                if (ex is SyncConfigError)
                {
                    ConsolePrinter.Error($"Configuration Error: {ex.Message}");
                    return;
                }

                // unhandled exception, rethrow
                throw;
            }
        } 
    }
}
