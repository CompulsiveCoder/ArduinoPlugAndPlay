using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddDeviceErrorRecoveryTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddDevice_ErrorRecovery ()
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

            deviceManager.DeviceAddedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script Intentionally Failed\" && exit 1";

            var assertion = new AssertionHelper (deviceManager);

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (info.Port);

            var countInFile = 0;

            for (int i = 1; i <= deviceManager.CommandRetryMax; i++) {
                deviceManager.RunLoop ();

                assertion.AssertDeviceExists (info);

                Assert.AreEqual (1, deviceManager.BackgroundStarter.StartedProcesses.Count, "Wrong number of processes found.");

                var process = deviceManager.BackgroundStarter.StartedProcesses ["add-" + info.Port];

                while (!process.HasExited)
                    Thread.Sleep (10);

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (i, countInFile);
            }
            
            // Make sure the process has stopped
            for (int i = 1; i <= 5; i++) {
                deviceManager.RunLoop ();

                countInFile = Convert.ToInt32 (File.ReadAllText (Path.GetFullPath ("fcf.txt")));

                Assert.AreEqual (deviceManager.CommandRetryMax, countInFile, "The number in the count file doesn't match.");

                assertion.AssertDeviceIsInList (info.Port);
            }

        }
    }
}

