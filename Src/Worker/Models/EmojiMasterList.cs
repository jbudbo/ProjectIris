using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Worker.Models
{
    /// <summary>
    /// A clean set of searchable Emoji data
    /// </summary>
    public sealed class EmojiMasterList : IReadOnlyCollection<string>
    {
        private readonly Dictionary<string, string> emojis = new();

        public EmojiMasterList(IEnumerable<EmojiData> emojiData)
        {
            foreach(var emoji in emojiData.Where(e => !string.IsNullOrWhiteSpace(e.unified)))
            {
                IEnumerable<string> emojiChars = emoji.unified
                    .Split('-')
                    .Select(hex => int.Parse(hex, NumberStyles.HexNumber))
                    .Select(char.ConvertFromUtf32);

                emojis.Add(string.Concat(emojiChars), emoji.name);
            }
        }

        /// <summary>
        /// Returns any Emojis from this master list found in the given Text
        /// </summary>
        /// <param name="text">The Text to identify Emojis in</param>
        /// <returns></returns>
        public IEnumerable<string> ContainsEmojis(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            foreach (var emoji in emojis)
            {
                if (text.Contains(emoji.Key))
                    yield return $"{emoji.Key} - {emoji.Value}";
            }
        }

        /// <summary>
        /// How many Emojis are in the list
        /// </summary>
        public int Count => emojis.Count;

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)emojis.Keys).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)emojis.Keys).GetEnumerator();
        }
    }
}