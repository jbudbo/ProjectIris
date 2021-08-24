using System.Collections;
using System.Globalization;

namespace Worker.Models
{
    internal sealed class EmojiMasterList : IReadOnlyCollection<string>
    {
        private readonly Dictionary<string, string> emojis = new();

        public EmojiMasterList(IEnumerable<EmojiData> emojiData)
        {
            foreach(var emoji in emojiData)
            {
                IEnumerable<string> emojiChars = emoji.unified
                    .Split('-')
                    .Select(hex => int.Parse(hex, NumberStyles.HexNumber))
                    .Select(char.ConvertFromUtf32);

                emojis.Add(string.Concat(emojiChars), emoji.name);
            }
        }

        public int Count => ((IReadOnlyCollection<string>)emojis).Count;

        public IEnumerator<string> GetEnumerator()
        {
            return ((IEnumerable<string>)emojis).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)emojis).GetEnumerator();
        }
    }
}