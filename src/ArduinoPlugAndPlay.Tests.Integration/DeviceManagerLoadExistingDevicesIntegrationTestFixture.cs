using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerLoadExistingDevicesIntegrationTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_LoadExistingDevices_PortDeviceMistmatch ()
        {
            var deviceManager = new DeviceManager ();

            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            //deviceManager.DeviceAddedCommand = "bash " + ProjectDirectory + "/count.sh";

            // Create two different example devices
            var deviceInfo = GetExampleDeviceInfo ();
            var deviceInfo2 = GetExampleDeviceInfo2 ();

            // Set the ports to be the same
            deviceInfo.Port = deviceInfo2.Port = "ttyUSB0";

            // Create the files for example device one
            CreateExampleDeviceInfoFiles (deviceInfo);

            // Set the mock output to device 2
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (deviceInfo2));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (deviceInfo.Port);

            // Run a loop to set off a long running command
            deviceManager.LoadExistingDeviceList ();


            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (deviceInfo, mockBackgroundProcessStarter);

            // Disconnect the device before the command is completed
            /*mockPlatformio.DisconnectDevice (deviceInfo.Port);

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

            Assert.AreEqual (2, mockBackgroundProcessStarter.CommandsRun.Count, "Invalid number of commands run.");*/
        }

    }
}

