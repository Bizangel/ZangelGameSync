using System.Security.Principal;

namespace ZangelGameSyncClient
{
    internal static class Utils
    {
        internal static bool IsCurrentProcessElevated()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        internal static bool IsFileWritable(string path)
        {
            try
            {
                using FileStream fs = new(path, FileMode.Open, FileAccess.Write);
                return fs.CanWrite;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

    }
}
