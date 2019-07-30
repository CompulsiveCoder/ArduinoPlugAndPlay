using System;
using NUnit.Framework;
using System.IO;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture (Category = "Unit")]
    public class DeviceInfoFileWriterTestFixture : BaseTestFixture
    {
        public DeviceInfoFileWriterTestFixture ()
        {
        }

        [Test]
        public void Test_WriteInfoToFile ()
        {
            var deviceDataDir = Path.GetFullPath ("devices");

            Directory.CreateDirectory (deviceDataDir);

            var writer = new DeviceInfoFileManager (deviceDataDir);

            var info = GetExampleDeviceInfo ();

            writer.WriteInfoToFile (info);

            var deviceInfoDir = Path.Combine (deviceDataDir, info.Port.Replace ("/dev/", ""));

            var familyNameFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "family.txt"));
            var groupNameFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "group.txt"));
            var projectNameFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "project.txt"));
            var boardTypeFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "board.txt"));
            var scriptCodeFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "script-code.txt"));
            var portFromFile = File.ReadAllText (Path.Combine (deviceInfoDir, "port.txt"));
            var addCommandCompletedFile = Convert.ToBoolean (File.ReadAllText (Path.Combine (deviceInfoDir, "add-command-completed.txt")));
            var removeCommandCompletedFile = Convert.ToBoolean (File.ReadAllText (Path.Combine (deviceInfoDir, "remove-command-completed.txt")));

            Assert.AreEqual (info.FamilyName, familyNameFromFile, "Incorrect family name.");
            Assert.AreEqual (info.GroupName, groupNameFromFile, "Incorrect group name.");
            Assert.AreEqual (info.ProjectName, projectNameFromFile, "Incorrect project name.");
            Assert.AreEqual (info.BoardType, boardTypeFromFile, "Incorrect board type.");
            Assert.AreEqual (info.ScriptCode, scriptCodeFromFile, "Incorrect script code.");
            Assert.AreEqual (info.Port, portFromFile, "Incorrect port.");
            Assert.AreEqual (info.AddCommandCompleted, addCommandCompletedFile, "Incorrect AddCommandCompleted value.");
            Assert.AreEqual (info.RemoveCommandCompleted, removeCommandCompletedFile, "Incorrect RemoveCommandCompleted value.");


        }

        [Test]
        public void Test_ReadInfoFromFile ()
        {
            Console.WriteLine ("Starting read info from files test...");

            var devicesInfoDir = Path.GetFullPath ("devices");

            Console.WriteLine ("Devices data dir: " + devicesInfoDir);

            Directory.CreateDirectory (devicesInfoDir);

            var port = "ttyUSB0";

            var deviceInfoDir = Path.Combine (devicesInfoDir, port);

            Console.WriteLine ("Device info dir: " + deviceInfoDir);

            Directory.CreateDirectory (deviceInfoDir);

            var familyName = "ExampleFamily";
            var groupName = "ExampleGroup";
            var projectName = "ExampleProject";
            var boardType = "uno";
            var scriptCode = "example-script";
            var addCommandCompleted = false;
            var removeCommandCompleted = false;

            File.WriteAllText (Path.Combine (deviceInfoDir, "family.txt"), familyName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "group.txt"), groupName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "project.txt"), projectName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "board.txt"), boardType);
            File.WriteAllText (Path.Combine (deviceInfoDir, "script-code.txt"), scriptCode);
            File.WriteAllText (Path.Combine (deviceInfoDir, "port.txt"), port);
            File.WriteAllText (Path.Combine (deviceInfoDir, "add-command-completed.txt"), addCommandCompleted.ToString ());
            File.WriteAllText (Path.Combine (deviceInfoDir, "remove-command-completed.txt"), removeCommandCompleted.ToString ());

            var writer = new DeviceInfoFileManager (devicesInfoDir);

            var info = writer.ReadInfoFromFile (port);

            Assert.AreEqual (familyName, info.FamilyName, "Incorrect family name.");
            Assert.AreEqual (groupName, info.GroupName, "Incorrect group name.");
            Assert.AreEqual (projectName, info.ProjectName, "Incorrect project name.");
            Assert.AreEqual (boardType, info.BoardType, "Incorrect board type.");
            Assert.AreEqual (scriptCode, info.ScriptCode, "Incorrect script code.");
            Assert.AreEqual (port, info.Port, "Incorrect port.");
            Assert.AreEqual (addCommandCompleted, info.AddCommandCompleted, "Incorrect AddCommandCompleted value.");
            Assert.AreEqual (removeCommandCompleted, info.RemoveCommandCompleted, "Incorrect RemoveCommandCompleted value.");

        }
    }
}

