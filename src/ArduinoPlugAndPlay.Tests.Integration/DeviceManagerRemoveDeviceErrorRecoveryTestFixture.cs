using System;
using NUnit.Framework;
using System.Threading;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerRemoveDeviceErrorRecoveryTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_RemoveDevice_ErrorRecovery ()
        {
            var info = GetExampleDeviceInfo ();

            // Set up the mock objects
            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            deviceManager.DeviceRemovedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script intentionally failed\" && exit 1 || exit 0";

            var assertion = new AssertionHelper (deviceManager);

            // Create example info files so it appears the device exists
            CreateExampleDeviceInfoFiles ();

            // Add the device to the ports list so it appears the device exists
            deviceManager.DevicePorts.Add (info.Port);

            // Disconnect the virtual (mock) device so it appears to have been removed
            mockPlatformio.DisconnectDevice (info.Port);

            var countInFile = 0;

            for (int i = 1; i <= deviceManager.CommandRetryMax; i++) {
                deviceManager.RunLoop ();

                assertion.AssertDeviceIsNotList (info.Port);

                Assert.AreEqual (1, deviceManager.BackgroundStarter.StartedProcesses.Count, "Wrong number of processes found.");

                var processWrapper = deviceManager.BackgroundStarter.StartedProcesses ["remove-" + info.Port];

                while (!processWrapper.Process.HasExited)
                    Thread.Sleep (100);

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (i, countInFile);
            }

            // Make sure the process has stopped
            for (int i = 1; i <= 5; i++) {
                deviceManager.RunLoop ();

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (deviceManager.CommandRetryMax, countInFile, "The number in the count file doesn't match.");

                assertion.AssertDeviceIsNotList (info.Port);
            }
        }

    }
}

