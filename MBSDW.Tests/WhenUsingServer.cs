using MBDSW;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace MBSDW.Tests
{
    public class WhenUsingServer
    {
        [Fact]
        public void Should_Start_Server()
        {
            // Arrange
            bool result;

            // Act
            using (var sut = new ServerManager())
            {
                sut.Start();
                result = sut.WaitUntil(ServerManager.ServerState.Running, 5000);
            }

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Should_Stop_Server()
        {
            // Arrange
            int result = 0;
            ServerManager.ServerState state;

            // Act
            using (var sut = new ServerManager())
            {
                sut.Start();
                Assert.True(sut.WaitUntil(ServerManager.ServerState.Running, 5000));
                sut.Stop();
                result = sut.Wait();
                state = sut.State;
            }

            // Assert
            Assert.Equal(ServerManager.ServerState.Stopped, state);
        }

        [Fact]
        public void Should_Start_Backup()
        {
            // Arrange

            // Act
            using (var sut = new ServerManager())
            {
                sut.Start();
                Assert.True(sut.WaitUntil(ServerManager.ServerState.Running, 5000));
                sut.Backup();
                sut.WaitForBackup(ServerManager.BackupState.Ready, 10000);
            }

            // Assert
        }
    }
}
