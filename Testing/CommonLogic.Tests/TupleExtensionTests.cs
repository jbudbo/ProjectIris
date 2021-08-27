
using Xunit;

namespace CommonLogic.Tests
{
    public class TupleExtensionTests
    {
        [Fact]
        public void Empty_Strings_Are_Handled()
        {
            var input = (1, "MOCK");
            var target = input.ToString(string.Empty);

            Assert.Equal(string.Empty, target);
        }

        [Fact]
        public void Format_Strings_Are_Honored()
        {
            var input = (1, "MOCK");
            var target = input.ToString("{0} {1}");

            Assert.Equal("1 MOCK", target);
        }
    }
}
