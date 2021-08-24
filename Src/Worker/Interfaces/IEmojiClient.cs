using System.Threading;
using System.Threading.Tasks;

namespace Worker.Interfaces
{
    using Models;

    internal interface IEmojiClient
    {
        Task<EmojiMasterList> DownloadEmojisAsync(CancellationToken cancellationToken = default);
    }
}