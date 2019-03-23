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
            Console.WriteLine ("");
            Console.WriteLine ("Preparing install from web test...");
            Console.WriteLine ("");

            PullFileFromProject ("scripts-web/install-from-web.sh", true);

            var scriptPath = Path.GetFullPath ("install-from-web.sh");

            var branch = new BranchDetector ().GetBranch ();


            var destination = "mock/install/ArduinoPlugAndPlay";

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, destination + "/is-mock-systemctl.txt");
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + destination;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            Console.WriteLine ("");
            Console.WriteLine ("Performing install from web test...");
            Console.WriteLine ("");

            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (ProjectDirectory, "mock/services/arduino-plug-and-play.service");

            Assert.IsTrue (File.Exists (expectedServiceFile), "Plug and play service file not found.");
        }
    }
}

