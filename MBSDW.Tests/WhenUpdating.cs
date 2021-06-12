using MBDSW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MBSDW.Tests
{
    public class WhenUpdating
    {
        [Fact]
        public void Should_Update_Successfully()
        {
            // Arrange
            Updater.ClearCurrentFilename();

            // Act
            var updateBefore = Updater.NeedsUpdate();
            Updater.Update();
            var updateAfter = Updater.NeedsUpdate();

            // Assert
            Assert.True(updateBefore);
            Assert.False(updateAfter);
        }
    }
}
