using System;
using NUnit.Framework;
using System.Threading;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerRemoveDeviceRetryAfterErrorTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_RemoveDevice_RetryAfterError ()
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

            deviceManager.USBDeviceDisconnectedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script intentionally failed\" && exit 1 || exit 0";

            var assertion = new AssertionHelper (deviceManager);

            // Create example info files so it appears the device exists
            CreateExampleDeviceInfoFiles ();

            // Add the device to the ports list so it appears the device exists
            deviceManager.DevicePorts.Add (info.Port);

            // Disconnect the virtual (mock) device so it appears to have been removed
            mockPlatformio.DisconnectDevice (info.Port);

            var countInFile = 0;

            ProcessWrapper processWrapper = null;

            for (int i = 1; i <= deviceManager.CommandRetryMax; i++) {
                deviceManager.RunLoop ();

                Thread.Sleep (1000);

                assertion.AssertDeviceIsNotList (info.Port);

                Assert.AreEqual (1, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Wrong number of processes found.");

                processWrapper = deviceManager.BackgroundStarter.QueuedProcesses.Peek ();

                while (!processWrapper.HasExited)
                    Thread.Sleep (100);

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (i, countInFile);
            }

            // The next loop should detect the failure
            deviceManager.RunLoop ();

            // The next loop should restart the process
            deviceManager.RunLoop ();

            Assert.IsTrue (processWrapper.Process.HasExited, "The process hasn't exited.");

            Assert.IsFalse (deviceManager.BackgroundStarter.QueuedProcesses.Contains (processWrapper), "The process still exists in the BackgroundProcessStarter.StartedProcesses list when it shouldn't be.");

            Assert.IsTrue (processWrapper.HasStarted, "The process hasn't started.");
        }

    }
}

