using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander
{
    public interface ICommandInvoker
    {
        Task InvokeAsync(InvokeRequest request, CancellationToken cancellationToken);
    }
}