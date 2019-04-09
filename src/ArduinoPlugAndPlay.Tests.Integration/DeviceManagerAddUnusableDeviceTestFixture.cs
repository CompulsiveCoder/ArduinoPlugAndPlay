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
            var mockOutput = "0";
            for (int i = 1; i < 100; i++) {
                mockOutput += "\n" + i;
            }
            mockReaderWriter.SetMockOutput (mockOutput);
            mockReaderWriter.EnableVirtualDelay = true; // This delay prevents the test log from getting bloated

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


        }
    }
}

