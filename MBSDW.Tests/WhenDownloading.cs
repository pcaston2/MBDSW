using MBDSW;
using System;
using Xunit;

namespace MBSDW.Tests
{
    public class WhenDownloading
    {
        [Fact]
        public void Should_Get_String_From_Url()
        {
            // Arrange

            // Act
            var result = WebGetUtil.GetStringContentFromUrl("http://www.httpbin.org");

            // Assert
            Assert.Contains("<title>httpbin.org</title>", result);
        }

        [Fact]
        public void Should_Get_Bytes_From_Url()
        {
            // Arrange

            // Act
            var result = WebGetUtil.GetByteContentFromUrl("http://www.httpbin.org");

            // Assert
            Assert.NotNull(result);
        }
    }
}
