using Iwate.Commander;

namespace Iwate.AzureFunctions.Middlewares.Commander
{
    internal static class CommandStoragePathResolverExtensions
    {
        internal static string GetLockFileName(this ICommandStoragePathResolver resolver, string? partition)
        {
            return resolver.GetQueueDirPath(partition).TrimEnd('/') + ".lock";
        }
    }
}
