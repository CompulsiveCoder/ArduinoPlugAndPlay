using System;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture (Category = "Unit")]
    public class DeviceManagerTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddDevice ()
        {
            var info = GetExampleDeviceInfo ();

            // Set up the mock objects
            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (info.Port);

            // Add the device
            deviceManager.AddDevice (info.Port);

            // Assert that the expected command was started
            assertion.AssertAddDeviceCommandStarted (info, mockBackgroundProcessStarter);

            assertion.AssertDeviceExists (info);

            assertion.AssertDeviceInfoFilesExist (deviceManager.Data.InfoDirectory, info);
        }


        [Test]
        public void Test_RemoveDevice ()
        {
            // Set up the mock objects
            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            var info = GetExampleDeviceInfo ();

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) device
            mockPlatformio.ConnectDevice (info.Port);

            // Add the device to the ports list so it appears the device exists
            deviceManager.DevicePorts.Add (info.Port);

            // Create the example device info files so it appears the device exists
            CreateExampleDeviceInfoFiles ();

            // Disconnect the virtual (mock) device so it appears to have been removed
            mockPlatformio.DisconnectDevice (info.Port);

            // Remove the device
            deviceManager.RemoveDevice (info.Port);

            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (info, mockBackgroundProcessStarter);

            // Assert that 0 devices are in the list
            assertion.AssertDeviceCount (0);

            // Assert that the info files have been removed
            assertion.AssertDeviceInfoFilesDontExist (deviceManager.Data.InfoDirectory, info);
        }

        [Test]
        public void Test_InsertValues ()
        {
            var deviceManager = new DeviceManager ();

            var startPattern = "{FAMILY}/{GROUP}/{PROJECT}/{BOARD}/{PORT}";

            var info = GetExampleDeviceInfo ();

            var expectedResult = info.FamilyName + "/" + info.GroupName + "/" + info.ProjectName + "/" + info.BoardType + "/" + info.Port.Replace ("/dev/", "");

            var actualResult = deviceManager.InsertValues (startPattern, info);

            Assert.AreEqual (expectedResult, actualResult, "Value insertion failed.");
                    
        }

        [Test]
        public void Test_CheckForChangedPorts ()
        {
            // Set up the mock objects
            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            // Set up the device manager with the mock dependencies
            var deviceManager = new DeviceManager ();
            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            var info = GetExampleDeviceInfo ();

            // Set the mock output from the device
            //mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) device
            mockPlatformio.ConnectDevice (info.Port);

            // Add the device to the ports list so it appears the device exists
            deviceManager.DevicePorts.Add (info.Port);

            // Create the example device info files so it appears the device exists
            CreateExampleDeviceInfoFiles ();

            var info2 = GetExampleDeviceInfo2 ();

            // Set the mock device info to a different device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info2));

            deviceManager.CheckForPortChanges ();

            // Assert that the expected command was started
            assertion.AssertRemoveDeviceCommandStarted (info, mockBackgroundProcessStarter);

        }
    }
}

