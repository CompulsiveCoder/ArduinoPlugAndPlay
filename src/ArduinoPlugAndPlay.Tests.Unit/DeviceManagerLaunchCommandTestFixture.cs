using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture]
    public class DeviceManagerLaunchCommandTestFixture : BaseTestFixture
    {
        public DeviceManagerLaunchCommandTestFixture ()
        {
        }

        [Test]
        public void Test_LaunchAddCommand_Timeout ()
        {
            var deviceManager = new DeviceManager ();

            var assertion = new AssertionHelper (deviceManager);

            var timeoutInSeconds = 3;

            //deviceManager.DeviceAddedCommand = "echo hello";
            deviceManager.DeviceAddedCommand = "bash " + ProjectDirectory + "/count.sh";
            deviceManager.CommandTimeoutInSeconds = timeoutInSeconds;

            var deviceInfo = GetExampleDeviceInfo ();

            deviceManager.LaunchAddDeviceCommand (deviceInfo);

            // Give the command time to run
            Thread.Sleep (4000);

            var logDir = Path.GetFullPath ("logs");
            Console.WriteLine (logDir);

            var logFilePath = "";
            foreach (var file in Directory.GetFiles(logDir, "*.txt"))
                logFilePath = file;

            Assert.IsNotNullOrEmpty (logFilePath, "Couldn't find log file.");

            var output = File.ReadAllText (logFilePath);

            Console.WriteLine ("Log file...");
            Console.WriteLine (output);
            Console.WriteLine ("");

            var unreachableNumber = timeoutInSeconds + 1;

            for (int i = 1; i <= timeoutInSeconds; i++) {
                Assert.IsTrue (output.Contains (i.ToString ()), "Script didn't output the expected value: " + i);
            }

            Assert.IsFalse (output.Contains (unreachableNumber.ToString ()), "Script continued past timeout.");
        }

        [Test]
        public void Test_LaunchRemoveCommand_Timeout ()
        {
            var deviceManager = new DeviceManager ();

            var assertion = new AssertionHelper (deviceManager);

            var timeoutInSeconds = 3;

            //deviceManager.DeviceRemovedCommand = "echo hello";
            deviceManager.DeviceRemovedCommand = "bash " + ProjectDirectory + "/count.sh";
            deviceManager.CommandTimeoutInSeconds = timeoutInSeconds;

            var deviceInfo = GetExampleDeviceInfo ();

            deviceManager.LaunchRemoveDeviceCommand (deviceInfo);

            // Give the command time to run
            Thread.Sleep (4000);

            var logDir = Path.GetFullPath ("logs");
            Console.WriteLine (logDir);

            var logFilePath = "";
            foreach (var file in Directory.GetFiles(logDir, "*.txt"))
                logFilePath = file;

            Assert.IsNotNullOrEmpty (logFilePath, "Couldn't find log file.");

            var output = File.ReadAllText (logFilePath);

            Console.WriteLine ("Log file...");
            Console.WriteLine (output);
            Console.WriteLine ("");

            var unreachableNumber = timeoutInSeconds + 1;

            for (int i = 1; i <= timeoutInSeconds; i++) {
                Assert.IsTrue (output.Contains (i.ToString ()), "Script didn't output the expected value: " + i);
            }

            Assert.IsFalse (output.Contains (unreachableNumber.ToString ()), "Script continued past timeout.");
        }
    }
}

