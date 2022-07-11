using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Ingress.Interfaces;

public interface ITwitterClient
{
    Task<StreamReader> GetReaderAsync(CancellationToken cancellationToken = default);
}
