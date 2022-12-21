using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Iwate.Commander
{
    internal class AzureBlobCommandStorage<TInvokeState> : ICommandStorage<TInvokeState>
        where TInvokeState : IInvokeState, new()
    {
        private readonly BlobContainerClient _container;
        private readonly string _queueDir;
        private readonly string _stateDir;
        public AzureBlobCommandStorage(string connectionString, string containerName, string queueDir, string stateDir) 
        {
            _container = new BlobContainerClient(connectionString, containerName);
            _queueDir = queueDir;
            _stateDir = stateDir;
        }

        public async Task InitAsync()
        {
            await _container.CreateIfNotExistsAsync().ConfigureAwait(false);
        }

        private string GetQueueDirPath(string partition)
        {
            if (string.IsNullOrEmpty(partition))
            {
                return $"{_queueDir}/shared/";
            }
            else
            {
                return $"{_queueDir}/partitioned/{partition}/";
            }
        }

        private string GetQueuePath(InvokeRequest request)
        {
            return $"{GetQueueDirPath(request.Partition)}{request.Id}@{request.Command}@{request.InvokedBy}";
        }

        private string GetStatePath(string id)
        {
            return $"{_stateDir}/{id}.json";
        }

        public async Task<string> EnqueueAsync(string partition, string user, string command, Stream payload, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var request = new InvokeRequest
            {
                Id = Ulid.NewUlid().ToString(),
                Partition = partition,
                Command = command,
                InvokedBy = user,
                Payload = payload,
            };

            var state = _container.GetBlockBlobClient(GetStatePath(request.Id));
            using (var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(new TInvokeState
            {
                Id = request.Id,
                Partition = partition,
                Command = command,
                InvokedBy = user,
                Status = InvokeStatus.Queuing,
            })))
            {
                await state.UploadAsync(stream, new BlobUploadOptions { }, cancellationToken);
            }

            var queue = _container.GetBlockBlobClient(GetQueuePath(request));
            await queue.UploadAsync(request.Payload, new BlobUploadOptions { }, cancellationToken);

            return request.Id;
        }

        public async Task<InvokeRequest> PeekAsync(string partition, CancellationToken cancellationToken)
        {
            var prefix = GetQueueDirPath(partition);
            var resultSegment = _container.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).AsPages(default, 1);
            var enumerator = resultSegment.GetAsyncEnumerator();

            if (await enumerator.MoveNextAsync())
            {
                foreach (BlobItem blobItem in enumerator.Current.Values)
                {
                    var response = await _container.GetBlockBlobClient(blobItem.Name).DownloadContentAsync(cancellationToken);
                    var name = blobItem.Name.Substring(prefix.Length).Split('@');
                    return new InvokeRequest
                    {
                        Partition = partition,
                        Id = name[0],
                        Command = name[1],
                        InvokedBy = name[2],
                        Payload = response.Value.Content.ToStream()
                    };
                }
            }

            return null;
        }

        public async Task RemoveAsync(InvokeRequest request, CancellationToken cancellationToken)
        {
            var queue = _container.GetBlockBlobClient(GetQueuePath(request));
            if (await queue.ExistsAsync(cancellationToken))
            {
                await queue.DeleteAsync(cancellationToken: cancellationToken);
            }
        }

        public async Task<TInvokeState> GetStateAsync(string id, CancellationToken cancellationToken)
        {
            var state = _container.GetBlockBlobClient(GetStatePath(id));

            if (!await state.ExistsAsync(cancellationToken))
            {
                return default(TInvokeState);
            }

            var result = await state.DownloadContentAsync(cancellationToken);

            return await JsonSerializer.DeserializeAsync<TInvokeState>(result.Value.Content.ToStream(), cancellationToken: cancellationToken);
        }

        public async Task SetStateAsync(TInvokeState state, CancellationToken cancellationToken)
        {
            var result = _container.GetBlockBlobClient(GetStatePath(state.Id));
            using (var stream = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(state)))
            {
                await result.UploadAsync(stream);
            }
        }
    }

    internal class InMemoryCommandStorage<TInvokeState> : ICommandStorage<TInvokeState>
        where TInvokeState : IInvokeState, new()
    {
        private readonly ConcurrentDictionary<(string, string, string), InvokeRequest> _commands 
            = new ConcurrentDictionary<(string, string, string), InvokeRequest>();
        private readonly ConcurrentDictionary<string, TInvokeState> _states
            = new ConcurrentDictionary<string, TInvokeState>();
        public Task InitAsync()
        {
            return Task.CompletedTask;
        }

        public Task<string> EnqueueAsync(string partition, string user, string command, Stream payload, CancellationToken cancellationToken)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var id = Ulid.NewUlid().ToString();
            var key = (id.ToString(), partition, command);
            var stream = new MemoryStream();
            payload.CopyTo(stream);
            stream.Seek(0, SeekOrigin.Begin);
            _commands.TryAdd(key, new InvokeRequest
            {
                Id = id,
                Partition = partition,
                Command = command,
                InvokedBy = user,
                Payload = stream,
            });
            _states.TryAdd(id.ToString(), new TInvokeState 
            { 
                Id = id,
                Partition = partition,
                Command = command,
                InvokedBy = user,
                Status = InvokeStatus.Queuing
            });
            return Task.FromResult(id);
        }

        public Task<InvokeRequest> PeekAsync(string partition, CancellationToken cancellationToken)
        {
            var queue = _commands.GetEnumerator();
            if (queue.MoveNext())
            {
                return Task.FromResult(queue.Current.Value);
            }
            return Task.FromResult<InvokeRequest>(null);
        }

        public Task RemoveAsync(InvokeRequest request, CancellationToken cancellationToken)
        {
            var key = (request.Id.ToString(), request.Partition, request.Command);
            if (_commands.TryRemove(key, out var remove))
            {
                remove.Payload?.Dispose();
            }
            return Task.CompletedTask;
        }

        public Task<TInvokeState> GetStateAsync(string id, CancellationToken cancellationToken)
        {
            if (_states.TryGetValue(id.ToString(), out var state))
            {
                return Task.FromResult(state);
            }
            return Task.FromResult(default(TInvokeState));
        }

        public Task SetStateAsync(TInvokeState state, CancellationToken cancellationToken)
        {
            _states.AddOrUpdate(state.Id.ToString(), key => state, (key,_) => state);
            return Task.CompletedTask;
        }
    }
}