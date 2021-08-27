using Xunit;

namespace CommonLogic.Tests
{
    public class StringExtensionTests
    {
        [Fact]
        public void Empty_Strings_Are_Handled()
        {

            var target = string.Empty.RaiseFirstChar();

            Assert.Equal(string.Empty, target);
        }

        [Fact]
        public void Single_Characters_Are_Handled()
        {

            var target = "m".RaiseFirstChar();

            Assert.Equal("M", target);
        }

        [Fact]
        public void Format_Strings_Are_Honored()
        {
            var target = "mock".RaiseFirstChar();

            Assert.Equal("Mock", target);
        }
    }
}
