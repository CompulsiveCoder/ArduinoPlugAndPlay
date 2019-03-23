using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Install.Web
{
    [TestFixture (Category = "InstallFromWeb")]
    public class InstallFromWebTestFixture : BaseInstallTestFixture
    {

        [Test]
        public void Test_Install_FromWeb ()
        {
            PullFileFromProject ("scripts-web/install-from-web.sh");

            var scriptPath = Path.Combine (ProjectDirectory, "install-from-web.sh");

            var branch = new BranchDetector ().GetBranch ();

            var destination = "mock/install/ArduinoPlugAndPlay";

            var cmd = "bash " + scriptPath + " " + branch + " " + destination;
            var starter = new ProcessStarter ();
            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (ProjectDirectory, "mock/services/arduino-plug-and-play.service");

            Assert.IsTrue (File.Exists (expectedServiceFile), "Plug and play service file not found.");
        }
    }
}

