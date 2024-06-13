namespace ZangelGameSyncClient
{
    // TODO think of a way to rewrite this handler.

    public class SyncClientExceptionHandler : IDisposable
    {
        private bool _disposed = false;

        public SyncClientExceptionHandler()
        {
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                // Clean up resources if needed
            }
        }

        public void Handle(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }
        }

        private void HandleException(Exception ex)
        {
            if (ex is SyncConfigError)
            {
                ConsolePrinter.Error($"Configuration error:\n{ex.Message}");
                return;
            }

            // unhandled, rethrow
            throw ex;
        }
    }
}
