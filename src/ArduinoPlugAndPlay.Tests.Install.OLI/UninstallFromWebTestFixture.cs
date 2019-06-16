﻿using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Install.OLI
{
    [TestFixture (Category = "OLI")]
    public class UninstallOLITestFixture : BaseInstallTestFixture
    {

        [Test]
        public void Test_Uninstall_OLI ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Preparing uninstall from web test...");
            Console.WriteLine ("");

            var branch = new BranchDetector ().Branch;

            var installDir = Path.GetFullPath ("installation/ArduinoPlugAndPlay");

            CreateDemoInstallation (branch, installDir);

            PullFileFromProject ("scripts-oli/uninstall.sh", true);

            var scriptPath = Path.GetFullPath ("uninstall.sh");

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, installDir + "/is-mock-systemctl.txt");
            Directory.CreateDirectory (Path.GetDirectoryName (isMockSystemCtlFile));
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + installDir;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            Console.WriteLine ("");
            Console.WriteLine ("Performing uninstall from web test...");
            Console.WriteLine ("");

            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var expectedServiceFile = Path.Combine (Path.Combine (TemporaryDirectory, installDir), "mock/services/arduino-plug-and-play.service");

            Assert.IsFalse (File.Exists (expectedServiceFile), "Plug and play service still exists when it should have been removed.");
        }

        public void CreateDemoInstallation (string branch, string installDir)
        {
            PullFileFromProject ("scripts-web/install-from-web.sh", true);

            var scriptPath = Path.GetFullPath ("install-from-web.sh");

            var destination = "installation/ArduinoPlugAndPlay";

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, destination + "/is-mock-systemctl.txt");
            Directory.CreateDirectory (Path.GetDirectoryName (isMockSystemCtlFile));
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + destination;

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
