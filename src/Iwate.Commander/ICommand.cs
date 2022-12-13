using System.Threading.Tasks;
using System.Threading;

namespace Iwate.Commander 
{
    public interface ICommand
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}

