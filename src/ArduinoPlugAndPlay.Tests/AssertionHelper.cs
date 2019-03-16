using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests
{
    public class AssertionHelper
    {
        public AssertionHelper ()
        {
        }

        public void AssertDeviceExists (DeviceInfo deviceInfo, DeviceManager deviceManager)
        {
            AssertDeviceIsInList (deviceInfo.Port, deviceManager);

            AssertDeviceInfoFilesExist (deviceManager.Data.InfoDirectory, deviceInfo);
        }

        public void AssertDeviceDoesntExist (DeviceInfo deviceInfo, DeviceManager deviceManager)
        {
            Assert.IsFalse (deviceManager.DevicePorts.Contains (deviceInfo.Port), "Device port is still found in DeviceManager list.");

            AssertDeviceInfoFilesDontExist (deviceManager.Data.InfoDirectory, deviceInfo);
        }

        public void AssertDeviceIsInList (string portName, DeviceManager deviceManager)
        {
            Assert.IsTrue (deviceManager.DevicePorts.Contains (portName));
        }

        public void AssertDeviceCount (int deviceCount, DeviceManager deviceManager)
        {
            var deviceList = deviceManager.GetDeviceList ();

            Assert.AreEqual (deviceCount, deviceList.Length, "Incorrect number of devices found in list.");

            var infoDir = deviceManager.Data.InfoDirectory;

            var numberOfDeviceDirectories = 0;
            if (Directory.Exists (infoDir))
                numberOfDeviceDirectories = Directory.GetDirectories (infoDir).Length;

            Assert.AreEqual (deviceCount, numberOfDeviceDirectories, "Incorrect number of device info directories found.");

            Console.WriteLine ("Device count is correct.");
        }

        public void AssertAddDeviceCommandStarted (DeviceInfo info, DeviceManager deviceManager, MockProcessStarter starter)
        {
            var expectedCommand = deviceManager.InsertValues (deviceManager.DeviceAddedCommand, info);
            AssertCommandStarted (starter, expectedCommand);
        }

        public void AssertRemoveDeviceCommandStarted (DeviceInfo info, DeviceManager deviceManager, MockProcessStarter starter)
        {
            var expectedCommand = deviceManager.InsertValues (deviceManager.DeviceRemovedCommand, info);
            AssertCommandStarted (starter, expectedCommand);
        }


        public void AssertCommandStarted (MockProcessStarter starter, string expectedCommand)
        {
            var lastCommandRun = starter.LastCommandRun;

            var fullExpectedCommand = "bash -c \"" + expectedCommand + "\"";

            Assert.AreEqual (fullExpectedCommand, lastCommandRun, "Commands don't match.");

            Console.WriteLine ("The expected bash command was started.");
        }

        public void AssertDeviceInfoFilesExist (string devicesDir, DeviceInfo info)
        {
            Assert.IsTrue (Directory.Exists (devicesDir), "Devices directory not found.");

            var deviceDir = Path.Combine (devicesDir, info.Port.Replace ("/dev/", ""));

            Assert.IsTrue (Directory.Exists (deviceDir), "Device directory not found.");

            var familyFilePath = Path.Combine (deviceDir, "family.txt");
            var groupFilePath = Path.Combine (deviceDir, "group.txt");
            var projectFilePath = Path.Combine (deviceDir, "project.txt");
            var boardFilePath = Path.Combine (deviceDir, "board.txt");
            var portFilePath = Path.Combine (deviceDir, "port.txt");

            Assert.IsTrue (File.Exists (familyFilePath), "family.txt file not found");
            Assert.IsTrue (File.Exists (groupFilePath), "project.txt file not found");
            Assert.IsTrue (File.Exists (projectFilePath), "group.txt file not found");
            Assert.IsTrue (File.Exists (boardFilePath), "board.txt file not found");
            Assert.IsTrue (File.Exists (portFilePath), "port.txt file not found");

            Assert.AreEqual (info.FamilyName, File.ReadAllText (familyFilePath).Trim (), "Family file content is incorrect.");
            Assert.AreEqual (info.GroupName, File.ReadAllText (groupFilePath).Trim (), "Group file content is incorrect.");
            Assert.AreEqual (info.ProjectName, File.ReadAllText (projectFilePath).Trim (), "Project file content is incorrect.");
            Assert.AreEqual (info.BoardType, File.ReadAllText (boardFilePath).Trim (), "Board file content is incorrect.");
            Assert.AreEqual (info.Port, File.ReadAllText (portFilePath).Trim (), "Port file content is incorrect.");

            Console.WriteLine ("Device info files exist and are accurate.");
        }

        public void AssertDeviceInfoFilesDontExist (string devicesDir, DeviceInfo info)
        {
            Assert.IsTrue (Directory.Exists (devicesDir), "Devices directory not found (it should still exist).");

            var deviceDir = Path.Combine (devicesDir, info.Port.Replace ("/dev/", ""));

            Assert.IsFalse (Directory.Exists (deviceDir), "Device directory found when it shouldn't be.");
        }
    }
}

