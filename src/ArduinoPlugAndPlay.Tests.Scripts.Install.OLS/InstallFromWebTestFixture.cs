﻿using System;
using NUnit.Framework;
using System.IO;
using ArduinoPlugAndPlay.Tests.Scripts.Install;

namespace ArduinoPlugAndPlay.Tests.Scripts.OLS
{
    [TestFixture (Category = "OLS")]
    public class InstallOLITestFixture : BaseInstallTestFixture
    {

        [Test]
        public void Test_Install_OLI ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Preparing install from web test...");
            Console.WriteLine ("");

            PullFileFromProject ("scripts-ols/install.sh", true);

            var scriptPath = Path.GetFullPath ("install.sh");

            var branch = new BranchDetector ().Branch;

            var destination = "installation/ArduinoPlugAndPlay";

            var smtpServer = "mail.newtestserver.com";

            var emailAddress = "user@newtestserver.com";

            // Configure systemctl mocking
            var isMockSystemCtlFile = Path.Combine (TemporaryDirectory, destination + "/is-mock-systemctl.txt");
            Directory.CreateDirectory (Path.GetDirectoryName (isMockSystemCtlFile));
            File.WriteAllText (isMockSystemCtlFile, 1.ToString ());

            var cmd = "bash " + scriptPath + " " + branch + " " + destination + " " + smtpServer + " " + emailAddress;

            Console.WriteLine ("Command:");
            Console.WriteLine ("  " + cmd);

            var starter = new ProcessStarter ();

            Console.WriteLine ("");
            Console.WriteLine ("Performing install from web test...");
            Console.WriteLine ("");

            starter.Start (cmd);

            Console.Write (starter.Output);

            Assert.IsFalse (starter.IsError, "An error occurred.");

            var installDir = Path.Combine (TemporaryDirectory, destination);

            Console.WriteLine ("");
            Console.WriteLine ("Checking service file was installed...");

            var expectedServiceFile = Path.Combine (installDir, "mock/services/arduino-plug-and-play.service");

            Assert.IsTrue (File.Exists (expectedServiceFile), "Plug and play service file not found: " + expectedServiceFile);

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
    }
}

