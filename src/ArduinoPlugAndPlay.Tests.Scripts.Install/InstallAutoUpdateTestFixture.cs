using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Scripts.Install
{
    [TestFixture (Category = "Install")]
    public class InstallAutoUpdateTestFixture : BaseInstallTestFixture
    {
        [Test]
        public void Test_InstallAutoUpdate ()
        {
            MoveToProjectDirectory ();

            var scriptPath = Path.Combine (ProjectDirectory, "install-auto-update.sh");

            var cmd = "sh " + scriptPath;
            var starter = GetTestProcessStarter ();
            starter.RunBash (cmd);

            Console.Write (starter.Starter.Output);

            Assert.IsFalse (starter.Starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (ProjectDirectory, "mock/services/arduino-plug-and-play.service");

            Assert.IsTrue (File.Exists (expectedServiceFile), "Plug and play service file not found.");
        }


    }
}

