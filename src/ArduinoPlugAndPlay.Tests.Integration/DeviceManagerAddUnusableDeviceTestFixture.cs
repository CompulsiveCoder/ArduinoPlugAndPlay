using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddUnusableDeviceTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddUnusableDevice ()
        {
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
            deviceManager.TimeoutExtractingDetailsInSeconds = 3;

            // deviceManager.DeviceAddedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script intentionally failed\" && exit 1";

            // Set the mock output from the device to something unrecognisable
            mockReaderWriter.SetMockOutput ("1\n2\n3\n4\n5\n6\n7\n8\n10");

            var port = "ttyUSB0";

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (port);

            // Run a loop which should detect the unusable device, and register it as unusable
            deviceManager.RunLoop ();

            // Ensure the device was added and registered as unusable
            Assert.AreEqual (1, deviceManager.DevicePorts.Count, "No ports added to the ports list");
            Assert.AreEqual (1, deviceManager.UnusableDevicePorts.Count, "No ports added to the unusable list");

            // Disconnect the unusable device
            mockPlatformio.DisconnectDevice (port);

            // Run a loop which should remove the unusable device completely
            deviceManager.RunLoop ();

            // Ensure the device was removed
            Assert.AreEqual (0, deviceManager.DevicePorts.Count, "Port wasnt removed from ports list after disconnect");
            Assert.AreEqual (0, deviceManager.UnusableDevicePorts.Count, "Port wasnt removed from unusable list after disconnect");


            /*var countInFile = 0;

            ProcessWrapper processWrapper = null;

            Console.WriteLine ("");
            Console.WriteLine ("Looping through " + deviceManager.CommandRetryMax + " retries...");
            Console.WriteLine ("");

            for (int i = 0; i <= deviceManager.CommandRetryMax; i++) {
                deviceManager.RunLoop ();

                assertion.AssertDeviceExists (info);

                Assert.AreEqual (1, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Wrong number of processes found.");

                processWrapper = deviceManager.BackgroundStarter.QueuedProcesses.Peek ();

                while (!processWrapper.Process.HasExited)
                    Thread.Sleep (100);

                Assert.AreEqual (1, processWrapper.Process.ExitCode, "The script should have intentionally exited with a code 1 (error).");

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (i + 1, countInFile);
            }

            Console.WriteLine ("");
            Console.WriteLine ("Running a loop which should abort and remove the process...");
            Console.WriteLine ("");

            // The next loop should detect the failure
            deviceManager.RunLoop ();

            // The next loop should restart the process
            deviceManager.RunLoop ();

            Assert.IsTrue (processWrapper.Process.HasExited, "The process hasn't exited.");

            Assert.IsFalse (deviceManager.BackgroundStarter.QueuedProcesses.Contains (processWrapper), "The process still exists in the BackgroundProcessStarter.StartedProcesses list when it shouldn't be.");

            Assert.IsTrue (processWrapper.HasStarted, "The process hasn't started.");*/
        }
    }
}

