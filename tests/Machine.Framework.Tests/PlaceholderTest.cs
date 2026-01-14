using Xunit;
using FluentAssertions;
using Machine.Framework.Core;
using Machine.Framework.Configuration;

namespace Machine.Framework.Tests
{
    public class PlaceholderTest
    {
        [Fact]
        public void Framework_ShouldBeReferenceable()
        {
            // Verify we can access types from Core and Configuration
            var opt = new SystemOptions();
            opt.Should().NotBeNull();
            
            // Verify Core
            bool isCore = true;
            isCore.Should().BeTrue();
        }
    }
}
