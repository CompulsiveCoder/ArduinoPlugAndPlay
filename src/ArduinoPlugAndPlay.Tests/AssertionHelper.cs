using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests
{
    public class AssertionHelper
    {
        public DeviceManager Manager;

        public AssertionHelper (DeviceManager deviceManager)
        {
            Manager = deviceManager;
        }

        public void AssertDeviceExists (DeviceInfo deviceInfo)
        {
            AssertDeviceIsInList (deviceInfo.Port);

            AssertDeviceInfoFilesExist (Manager.Data.InfoDirectory, deviceInfo);
        }

        public void AssertDeviceDoesntExist (DeviceInfo deviceInfo)
        {
            Assert.IsFalse (Manager.DevicePorts.Contains (deviceInfo.Port), "Device port is still found in DeviceManager list.");

            AssertDeviceInfoFilesDontExist (Manager.Data.InfoDirectory, deviceInfo);
        }

        public void AssertDeviceIsInList (string portName)
        {
            Assert.IsTrue (Manager.DevicePorts.Contains (portName), "Device wasn't found in the list: " + portName);
        }

        public void AssertDeviceIsNotList (string portName)
        {
            Assert.IsFalse (Manager.DevicePorts.Contains (portName), "Device was found in the list when it shouldn't be: " + portName);
        }

        public void AssertDeviceCount (int deviceCount)
        {
            var deviceList = Manager.GetDeviceList ();

            Assert.AreEqual (deviceCount, deviceList.Length, "Incorrect number of devices found in list.");

            var infoDir = Manager.Data.InfoDirectory;

            var numberOfDeviceDirectories = 0;
            if (Directory.Exists (infoDir))
                numberOfDeviceDirectories = Directory.GetDirectories (infoDir).Length;

            Assert.AreEqual (deviceCount, numberOfDeviceDirectories, "Incorrect number of device info directories found.");

            Console.WriteLine ("Device count is correct.");
        }

        public void AssertAddDeviceCommandStarted (DeviceInfo info, MockBackgroundProcessStarter starter)
        {
            var expectedCommand = Manager.InsertValues (Manager.DeviceAddedCommand, info);
            AssertCommandStarted (starter, expectedCommand, Manager.GetLogFile ("add", info));
        }

        public void AssertRemoveDeviceCommandStarted (DeviceInfo info, MockBackgroundProcessStarter starter)
        {
            var expectedCommand = Manager.InsertValues (Manager.DeviceRemovedCommand, info);
            AssertCommandStarted (starter, expectedCommand, Manager.GetLogFile ("remove", info));
        }

        public void AssertCommandStarted (MockBackgroundProcessStarter starter, string expectedCommand, string logFile)
        {
            var fullExpectedCommand = "timeout " + Manager.CommandTimeoutInSeconds + "s /bin/bash -c '" + expectedCommand.Replace ("'", "\\'") + " >> " + logFile + "'";

            Assert.IsTrue (starter.CommandsRun.Contains (fullExpectedCommand), "The expected command wasn't run.");

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

