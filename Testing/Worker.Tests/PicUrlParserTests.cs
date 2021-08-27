using Xunit;

namespace Worker.Tests;

using Support;

public class PicUrlParserTests
{
    [Fact]
    [Trait("path","happy")]
    public void Parser_Handles_Missing_Schema()
    {
        var parser = new PicUrlParser();
        var uri = new Uri("localhost/Home/Base", UriKind.Relative);

        string host = parser.GetHost(uri);

        Assert.Equal("localhost", host);
    }

    [Fact]
    [Trait("path", "happy")]
    public void Parser_Handles_Full_Uri()
    {
        var parser = new PicUrlParser();
        var uri = new Uri("http://localhost/Home/Base", UriKind.Absolute);

        string host = parser.GetHost(uri);

        Assert.Equal("localhost", host);
    }

    [Fact]
    [Trait("path", "happy")]
    public void Parser_Returns_No_String_If_No_Host()
    {
        var parser = new PicUrlParser();
        var uri = new Uri("/Home/Base", UriKind.Relative);

        string host = parser.GetHost(uri);

        Assert.Equal(string.Empty, host);
    }
}