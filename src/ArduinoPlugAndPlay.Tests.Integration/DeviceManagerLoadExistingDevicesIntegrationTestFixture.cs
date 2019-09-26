using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerLoadExistingDevicesIntegrationTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_LoadExistingDevices_LoadsExistingDevicesWithoutRecreatingThem ()
        {
            var deviceManager = new DeviceManager ();

            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockSerialDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();
            var mockSerialPortWrapper = new MockSerialPortWrapper ();

            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;
            deviceManager.SerialPort = mockSerialPortWrapper;

            var assertion = new AssertionHelper (deviceManager);

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            //deviceManager.DeviceAddedCommand = "bash " + ProjectDirectory + "/count.sh";

            Console.WriteLine ("Creating example device info files...");
            var deviceInfo = GetExampleDeviceInfo (0);
            CreateExampleDeviceInfoFiles (deviceInfo);

            Console.WriteLine ("Creating example device 2 info files...");
            var deviceInfo2 = GetExampleDeviceInfo (1);
            CreateExampleDeviceInfoFiles (deviceInfo2);

            Console.WriteLine ("Setting example device 1 as mock serial device output...");
            mockReaderWriter.SetMockOutput (deviceInfo.Port, MockOutputs.GetDeviceSerialOutput (deviceInfo));

            Console.WriteLine ("Virtually connecting device 1...");
            mockPlatformio.ConnectDevice (deviceInfo.Port);
            mockSerialPortWrapper.ConnectDevice (deviceInfo.Port);

            Console.WriteLine ("Setting example device 2 as mock serial device output...");
            mockReaderWriter.SetMockOutput (deviceInfo2.Port, MockOutputs.GetDeviceSerialOutput (deviceInfo2));

            Console.WriteLine ("Virtually connecting device 2...");
            mockPlatformio.ConnectDevice (deviceInfo2.Port);
            mockSerialPortWrapper.ConnectDevice (deviceInfo2.Port);

            Console.WriteLine ("Loading existing device info from file...");
            deviceManager.LoadExistingDeviceListFromFiles ();

            Console.WriteLine ("Checking that devices were loaded...");
            Assert.AreEqual (2, deviceManager.DevicePorts.Count, "Incorrect number of devices loaded");

            Console.WriteLine ("Running loop to see if commands are launched...");
            deviceManager.RunLoop ();

            Assert.AreEqual (0, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Processes started when they shouldn't have.");


            // Assert that the expected command was started
            //assertion.AssertRemoveDeviceCommandStarted (deviceInfo, mockBackgroundProcessStarter);

        }

        [Test]
        public void Test_LoadExistingDevices_PortDeviceMistmatch ()
        {
            var deviceManager = new DeviceManager ();

            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockSerialDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            // Create two different example devices
            var deviceInfo = GetExampleDeviceInfo (0);
            var deviceInfo2 = GetExampleDeviceInfo (1);

            // Set the ports to be the same
            deviceInfo.Port = deviceInfo2.Port = "ttyUSB0";

            // Create the files for example device one
            CreateExampleDeviceInfoFiles (deviceInfo);

            // Set the mock output to device 2
            mockReaderWriter.SetMockOutput (deviceInfo2.Port, MockOutputs.GetDeviceSerialOutput (deviceInfo2));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (deviceInfo.Port);

            // Run a loop to set off a long running command
            deviceManager.LoadExistingDeviceListFromFiles ();

            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (deviceInfo, mockBackgroundProcessStarter);
        }
    }
}