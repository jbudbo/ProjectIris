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

        public IEnumerable<string> ContainsEmojis(string text)
        {
            foreach (var emoji in emojis)
            {
                if (text.Contains(emoji.Key))
                    yield return $"{emoji.Key} - {emoji.Value}";
            }
        }

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