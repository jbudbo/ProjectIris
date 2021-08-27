
using Xunit;

namespace Worker.Tests;

using Models;

public class EmojiMasterListTests
{
    [Fact]
    public void Master_List_Provides_Keys()
    {
        var emojiData = new EmojiData("MOCK", "261D-FE0F", string.Empty, string.Empty, string.Empty);
        EmojiMasterList list = new (new[] { emojiData });

        Assert.NotEmpty(list);

        string only = list.First();

        Assert.Equal("☝️", only);
    }

    [Fact]
    public void Master_List_Finds_Emojis()
    {
        var emojiData = new EmojiData("MOCK", "261D-FE0F", string.Empty, string.Empty, string.Empty);
        EmojiMasterList list = new(new[] { emojiData });

        var emojisFromList = list.ContainsEmojis("Don't look ☝️")
            .ToArray();

        Assert.NotEmpty(emojisFromList);

        Assert.Equal("☝️ - MOCK", emojisFromList[0]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Master_List_Handles_No_String(string text)
    {
        var emojiData = new EmojiData("MOCK", "261D-FE0F", string.Empty, string.Empty, string.Empty);
        EmojiMasterList list = new(new[] { emojiData });

        var emojisFromList = list.ContainsEmojis(text)
            .ToArray();

        Assert.Empty(emojisFromList);
    }

    [Theory]
    [InlineData("No Emojis Here!")]
    [InlineData("Hmmmm 㘝️")]
    public void Master_List_Handles_String_With_No_Or_Unrecognized_Emoji(string text)
    {
        var emojiData = new EmojiData("MOCK", "261D-FE0F", string.Empty, string.Empty, string.Empty);
        EmojiMasterList list = new(new[] { emojiData });

        var emojisFromList = list.ContainsEmojis(text)
            .ToArray();

        Assert.Empty(emojisFromList);
    }
}

