using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddDeviceRetryAfterErrorTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddDevice_RetryAfterError ()
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

            deviceManager.DeviceAddedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script intentionally failed\" && exit 1";

            var assertion = new AssertionHelper (deviceManager);

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (info.Port);

            var countInFile = 0;

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

            deviceManager.RunLoop ();

            Assert.IsTrue (processWrapper.Process.HasExited, "The process hasn't exited.");

            Assert.IsFalse (deviceManager.BackgroundStarter.QueuedProcesses.Contains (processWrapper), "The process still exists in the BackgroundProcessStarter.StartedProcesses list when it shouldn't be.");
        }
    }
}

