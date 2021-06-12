using MBDSW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MBSDW.Tests
{
    public class WhenParsing
    {
        [Fact]
        public void Should_Find_Filename()
        {
            // Arrange
            var content =
            #region content
               @"                            <div class=""card-footer"">" +
                            @"" +
                            @"                                    <div class=""check-to-proceed px-3"">" +
                            @"                                        <label class=""form-check-label bg-fade-linear p-2"">" +
                            @"                                            <input type=""checkbox"" class=""form-check-input""/>" +
                            @"                                            I agree to the <a href=""https://minecraft.net/terms"">Minecraft End User License Agreement</a> and <a href=""https://go.microsoft.com/fwlink/?LinkId=521839"">Privacy Policy</a>" +
                            @"" +
                            @"                                        </label>" +
                            @"                                        <a href=""https://minecraft.azureedge.net/bin-win/bedrock-server-1.16.221.01.zip"" class=""btn btn-disabled-outline mt-4 downloadlink"" role=""button"" data-platform=""serverBedrockWindows"" tabindex=""-1"">Download </a>" +
                            @"                                    </div>" +
                                                            @"" +
                                                            @"" +
                                                            @"" +
                                                            @"" +
                            @"                            </div>" +
                            @"                        </div>" +
                                                @"" +
                            @"                        <div class=""card bg-white text-center px-4 py-5"">" +
                            @"                            <div class=""grow""></div>" +
                                                        @"" +
                            @"                       <span class=""ribbon none"">" +
                            @"                          <svg class=""ribbon-icon"">" +
                            @"                             <use xlink:href=""#ribbon""></use>" +
                            @"                          </svg>" +
                            @"                          <span class=""ribbon-label text-uppercase text-small letter-spacing-1 text-white font-weight-bold"">Ubuntu</span>" +
                            @"                       </span>" +
                                                        @"" +
                                                        @"" +
                                                        @"" +
                                                        @"" +
                            @"                                <div class=""card-body"">" +
                            @"                                    <h2 class=""card-title px-5 pt-5 pb-2"">Ubuntu Server software for Ubuntu</h2>" +
                            @"                                    <p>Unzip the container file into an empty folder. Start the server with the following command:</p>" +
                            @"<p><b data-rte-class=""rte-temp""><span class=""bedrock-server"">LD_LIBRARY_PATH=. ./bedrock_server</span></b></p>" +
                            @"<p>Follow the bundled how to guide to configure the server.</p>" +
                            @"" +
                            @"                                </div>" +
                                                        @"" +
                            @"" +
                            @"                            <div class=""card-footer"">" +
                                                            @"" +
                            @"                                    <div class=""check-to-proceed px-3"">" +
                            @"                                        <label class=""form-check-label bg-fade-linear p-2"">" +
                            @"                                            <input type=""checkbox"" class=""form-check-input""/>" +
                            @"                                            I agree to the <a href=""https://minecraft.net/terms"">Minecraft End User License Agreement</a> and <a href=""https://go.microsoft.com/fwlink/?LinkId=521839"">Privacy Policy</a>" +
                            @"" +
                            @"                                        </label>" +
                            @"                                        <a href=""https://minecraft.azureedge.net/bin-linux/bedrock-server-1.16.221.01.zip"" class=""btn btn-disabled-outline mt-4 downloadlink"" role=""button"" data-platform=""serverBedrockLinux"" tabindex=""-1"">Download </a>" +
                            @"                                    </div>" +
                                                            @"" +
                                                            @"" +
                                                            @"" +
                                                            @"" +
                            @"                            </div>" +
                            @"                        </div>" +
                                                @"" +
                            @"                </div>" +
                            @"            </div>" +
                            @"        </div>";
            #endregion

            // Act

            var result = RegexUtil.FindInString(content, @"<a href=""(?<filename>[^\s]*?bin-win[^\s]*?.zip)"".*?>", "filename");

            // Assert

            Assert.Equal("https://minecraft.azureedge.net/bin-win/bedrock-server-1.16.221.01.zip", result);
        }
    }
}
