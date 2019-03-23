using System;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerIntegrationTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_ConnectDevice ()
        {
            Console.WriteLine ("Testing connecting an arduino device...");

            // Set up the mock objects
            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockProcessStarter = new MockProcessStarter ();

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.Starter = mockProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            // Assert that no devices are detected by default
            assertion.AssertDeviceCount (0);

            for (int i = 0; i < 3; i++) {
                Console.WriteLine ("");
                Console.WriteLine ("Connecting device #" + i + "...");
                Console.WriteLine ("");

                var info = GetExampleDeviceInfo (i);

                // Virtually connect a device
                mockPlatformio.ConnectDevice (info.Port);

                // Set the mock output from the device
                mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

                // Run a device manager loop
                deviceManager.RunLoop ();

                // Assert that the expected command was started
                assertion.AssertAddDeviceCommandStarted (info, mockProcessStarter);

                // Assert there is 1 device
                assertion.AssertDeviceCount (i + 1);

                // Assert that the device related data/info was created
                assertion.AssertDeviceExists (info);
            }


            Console.WriteLine ("");
            Console.WriteLine ("Disconnecting device #1...");
            Console.WriteLine ("");

            var info1 = GetExampleDeviceInfo (1);

            // Virtually connect a device
            mockPlatformio.DisconnectDevice (info1.Port);

            // Run a device manager loop
            deviceManager.RunLoop ();

            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (info1, mockProcessStarter);

            // Assert there are 2 devices left
            assertion.AssertDeviceCount (2);

            // Assert that the device related data/info was removed
            assertion.AssertDeviceDoesntExist (info1);
        }

    }
}

