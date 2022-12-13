using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander
{
    public class Commander<TInvokeState> where TInvokeState : IInvokeState
    {
        private readonly ICommandStorage<TInvokeState> _storage;
        private readonly ICommandInvoker _invoker;
        public Commander(ICommandStorage<TInvokeState> storage, ICommandInvoker invoker)
        {
            _storage = storage;
            _invoker = invoker;
        }

        public Task InitAsync()
            => _storage.InitAsync();
        
        public Task<InvokeId> EnqueueAsync(string partition, string command, Stream payload, CancellationToken cancellationToken)
            => _storage.EnqueueAsync(partition, command, payload, cancellationToken);
        
        public Task<InvokeRequest> PeekAsync(string partition, CancellationToken cancellationToken)
            => _storage.PeekAsync(partition, cancellationToken);

        public Task RemoveAsync(InvokeRequest request, CancellationToken cancellationToken)
            => _storage.RemoveAsync(request, cancellationToken);


        public Task<TInvokeState> GetStateAsync(InvokeId id, CancellationToken cancellationToken)
            => _storage.GetStateAsync(id, cancellationToken);
        
        public Task SetStateAsync(TInvokeState state, CancellationToken cancellationToken)
            => _storage.SetStateAsync(state, cancellationToken);

        public Task InvokeAsync(InvokeRequest request, CancellationToken cancellationToken)
            => _invoker.InvokeAsync(request, cancellationToken);
    }

    public class Commander : Commander<InvokeState>
    {
        public Commander(ICommandStorage<InvokeState> storage, ICommandInvoker invoker) 
            : base(storage, invoker)
        {
        }
    }
}
