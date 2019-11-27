using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddDeviceThenRemoveBeforeFinishedTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddThenRemoveDeviceBeforeFinish ()
        {
            var deviceManager = new DeviceManager ();

            // TODO: Remove if not needed. Should be obsolete.
            //var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockSerialDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();
            var mockSerialPortWrapper = new MockSerialPortWrapper ();

            // TODO: Remove if not needed. Should be obsolete.
            //deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;
            deviceManager.SerialPort = mockSerialPortWrapper;

            var assertion = new AssertionHelper (deviceManager);

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            deviceManager.USBDeviceConnectedCommand = "bash " + ProjectDirectory + "/count.sh";

            var deviceInfo = GetExampleDeviceInfo ();

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (deviceInfo.Port, MockOutputs.GetDeviceSerialOutput (deviceInfo));

            // Connect the virtual (mock) USB device
            // TODO: Remove if not needed. Should be obsolete.
            //mockPlatformio.ConnectDevice (deviceInfo.Port);
            mockSerialPortWrapper.ConnectDevice (deviceInfo.Port);

            // Run a loop to set off a long running command
            deviceManager.RunLoop ();

            // Give the command time to start
            Thread.Sleep (2000);

            // Disconnect the device before the command is completed
            // TODO: Remove if not needed. Should be obsolete.
            //mockPlatformio.DisconnectDevice (deviceInfo.Port);
            mockSerialPortWrapper.DisconnectDevice (deviceInfo.Port);

            // Run another loop to see how it handles it
            deviceManager.RunLoop ();

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

            var reachableNumber = 1;

            Assert.IsTrue (output.Contains (reachableNumber.ToString ()), "Script didn't output anything.");

            var unreachableNumber = 5;

            Assert.IsFalse (output.Contains (unreachableNumber.ToString ()), "Script continued too far.");

            Assert.AreEqual (2, mockBackgroundProcessStarter.CommandsRun.Count, "Invalid number of commands run.");
        }
    }
}

