using Microsoft.Azure.Functions.Worker;

namespace Iwate.AzureFunctions.Middlewares.Commander
{
    public static class FunctionContextExtensions
    {
        private const string KEY = "Iwate.Commander.InvokeRequestBase.Partition"; 
        internal static void SetCommanderTriggeredPartition(this FunctionContext context, string? partition)
        {
            context.Items[KEY] = partition ?? new object();
        }
        public static string? GetCommanderTriggeredPartition(this FunctionContext context)
        {
            return context.Items[KEY] as string;
        }
    }
}
