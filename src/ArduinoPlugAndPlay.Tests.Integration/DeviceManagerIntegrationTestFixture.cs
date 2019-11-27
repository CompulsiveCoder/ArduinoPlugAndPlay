using System;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerIntegrationTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_Connect3DevicesThenRemove1 ()
        {
            Console.WriteLine ("Testing connecting an arduino device...");

            // Set up the mock objects
            // TODO: Remove if not needed. Should be obsolete.
            //var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockSerialDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();
            var mockSerialPortWrapper = new MockSerialPortWrapper ();

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            // TODO: Remove if not needed. Should be obsolete.
            //deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;
            deviceManager.SerialPort = mockSerialPortWrapper;

            var assertion = new AssertionHelper (deviceManager);

            // Assert that no devices are detected by default
            assertion.AssertDeviceCount (0);

            for (int i = 0; i < 3; i++) {
                Console.WriteLine ("");
                Console.WriteLine ("Connecting device #" + i + "...");
                Console.WriteLine ("");

                var deviceInfo = GetExampleDeviceInfo (i);

                // Virtually connect a device
                //mockPlatformio.ConnectDevice (deviceInfo.Port);
                mockSerialPortWrapper.ConnectDevice (deviceInfo.Port);

                // Set the mock output from the device
                mockReaderWriter.SetMockOutput (deviceInfo.Port, MockOutputs.GetDeviceSerialOutput (deviceInfo));

                // Run a device manager loop
                deviceManager.RunLoop ();

                // Assert that the expected command was started
                assertion.AssertAddDeviceCommandStarted (deviceInfo, mockBackgroundProcessStarter);

                // Assert there is 1 device
                assertion.AssertDeviceCount (i + 1);

                // Assert that the device related data/info was created
                assertion.AssertDeviceExists (deviceInfo);
            }


            Console.WriteLine ("");
            Console.WriteLine ("Disconnecting device #1...");
            Console.WriteLine ("");

            var deviceInfo1 = GetExampleDeviceInfo (1);

            // Virtually connect a device
            mockSerialPortWrapper.DisconnectDevice (deviceInfo1.Port);
            // TODO: Remove if not needed. Should be obsolete.
            //mockPlatformio.DisconnectDevice (deviceInfo1.Port);

            // Run a device manager loop
            deviceManager.RunLoop ();

            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (deviceInfo1, mockBackgroundProcessStarter);

            // Assert there are 2 devices left
            assertion.AssertDeviceCount (2);

            // Assert that the device related data/info was removed
            assertion.AssertDeviceDoesntExist (deviceInfo1);
        }
    }
}