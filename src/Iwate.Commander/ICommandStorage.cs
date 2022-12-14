using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander
{
    public interface ICommandStorage<TInvokeState>
        where TInvokeState : IInvokeState
    {
        Task InitAsync();
        Task<string> EnqueueAsync(string partition, string user, string command, Stream payload, CancellationToken cancellationToken);
        Task<InvokeRequest> PeekAsync(string partition, CancellationToken cancellationToken);
        Task RemoveAsync(InvokeRequest request, CancellationToken cancellationToken);
        Task<TInvokeState> GetStateAsync(string id, CancellationToken cancellationToken);
        Task SetStateAsync(TInvokeState state, CancellationToken cancellationToken);
    }
}