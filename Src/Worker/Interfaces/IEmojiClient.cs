namespace Worker.Interfaces
{
    using Models;

    internal interface IEmojiClient
    {
        Task<EmojiData[]> DownloadEmojisAsync(CancellationToken cancellationToken);
    }
}