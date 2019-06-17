using System;
using NUnit.Framework;
using System.IO;
using ArduinoPlugAndPlay.Tests.Scripts.Install;

namespace ArduinoPlugAndPlay.Tests.Scripts.OLS
{
    [TestFixture (Category = "OLS")]
    public class ReinstallOLSTestFixture : BaseInstallTestFixture
    {

        [Test]
        public void Test_Reinstall_OLS ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Preparing reinstall test...");
            Console.WriteLine ("");

            var branch = new BranchDetector ().Branch;

            var installDir = Path.GetFullPath ("installation/ArduinoPlugAndPlay");

            var smtpServer = "mail.newtestserver.com";

            var emailAddress = "user@newtestserver.com";

            CreateDemoInstallation (branch, installDir, smtpServer, emailAddress);

            PullFileFromProject ("scripts-ols/reinstall.sh", true);

            var scriptPath = Path.GetFullPath ("reinstall.sh");

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, installDir + "/is-mock-systemctl.txt");
            Directory.CreateDirectory (Path.GetDirectoryName (isMockSystemCtlFile));
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + installDir;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            Console.WriteLine ("");
            Console.WriteLine ("Performing reinstall test...");
            Console.WriteLine ("");

            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (Path.Combine (TemporaryDirectory, installDir), "mock/services/arduino-plug-and-play.service");

            Assert.IsFalse (File.Exists (expectedServiceFile), "Plug and play service still exists when it should have been removed.");

            Console.WriteLine ("");
            Console.WriteLine ("Checking config file was installed...");

            var configFile = Path.Combine (installDir, "ArduinoPlugAndPlay.exe.config");

            Assert.IsTrue (File.Exists (configFile), "ArduinoPlugAndPlay.exe.config file not found at: " + configFile);

            Console.WriteLine ("");
            Console.WriteLine ("Checking email details were installed...");

            var configFileContent = File.ReadAllText (configFile);

            Assert.IsTrue (configFileContent.Contains (smtpServer), "SMTP server wasn't injected into the config file.");
            Assert.IsTrue (configFileContent.Contains (emailAddress), "Email address wasn't injected into the config file.");
        }

        public void CreateDemoInstallation (string branch, string installDir, string smtpServer, string emailAddress)
        {
            PullFileFromProject ("scripts-ols/install.sh", true);

            var scriptPath = Path.GetFullPath ("install.sh");

            var destination = "installation/ArduinoPlugAndPlay";

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, destination + "/is-mock-systemctl.txt");
            Directory.CreateDirectory (Path.GetDirectoryName (isMockSystemCtlFile));
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + destination + " " + smtpServer + " " + emailAddress;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (Path.Combine (TemporaryDirectory, destination), "mock/services/arduino-plug-and-play.service");

            Assert.IsTrue (File.Exists (expectedServiceFile), "Plug and play service file not found.");
        }
    }
}

