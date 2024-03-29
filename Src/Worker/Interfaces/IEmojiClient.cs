﻿namespace Worker.Interfaces
{
    using Models;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// An HTTP Client used to download and provide an <see cref="EmojiMasterList"/>
    /// </summary>
    public interface IEmojiClient
    {
        /// <summary>
        /// Downloads Emoji data and provides back the information in a <see cref="EmojiMasterList"/>
        /// </summary>
        /// <param name="cancellationToken">A token for stopping any downloads</param>
        /// <returns></returns>
        Task<EmojiMasterList> DownloadEmojisAsync(CancellationToken cancellationToken = default);
    }
}