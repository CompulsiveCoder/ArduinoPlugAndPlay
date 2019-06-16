using System;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ArduinoPlugAndPlay.Tests
{
    public class BaseTestFixture
    {
        public string ProjectDirectory { get; set; }

        public string TemporaryDirectory { get; set; }

        public MockDeviceOutputs MockOutputs = new MockDeviceOutputs ();

        public BaseTestFixture ()
        {
        }

        [SetUp]
        public virtual void Initialize ()
        {
            // TODO: Reorganize this. Some tests move back to the project directory so it's wasteful
            // to generate the temporary directory if it isn;t being used.
            InitializeProjectDirectory ();

            MoveToTemporaryDirectory ();

            Console.WriteLine ("");
            Console.WriteLine ("====================");
            Console.WriteLine ("Preparing test");
            Console.WriteLine (TestContext.CurrentContext.Test.FullName);


        }

        public void InitializeProjectDirectory ()
        {
            ProjectDirectory = Environment.CurrentDirectory;

            ProjectDirectory = ProjectDirectory.Replace ("/bin/Debug", "");
            ProjectDirectory = ProjectDirectory.Replace ("/bin/Release", "");
        }

        [TearDown]
        public virtual void Finish ()
        {
            HandleFailureFile ();

            Console.WriteLine ("Finished test");
            Console.WriteLine ("====================");
            Console.WriteLine ("");

            CleanTemporaryDirectory ();
        }

        #region Process Starter

        public TestProcessStarter GetTestProcessStarter ()
        {
            return GetTestProcessStarter (true);
        }

        public TestProcessStarter GetTestProcessStarter (bool initializeStarter)
        {
            var starter = new TestProcessStarter ();

            starter.WorkingDirectory = ProjectDirectory;

            if (initializeStarter)
                starter.Initialize ();

            return starter;
        }

        #endregion

        public void HandleFailureFile ()
        {
            var failuresDir = Path.GetFullPath ("../../failures");

            var fixtureName = TestContext.CurrentContext.Test.FullName;

            var failureFile = Path.Combine (failuresDir, fixtureName + ".txt");

            if (TestContext.CurrentContext.Result.State == TestState.Error
                || TestContext.CurrentContext.Result.State == TestState.Failure) {
                Console.WriteLine ("Test failed.");

                Console.WriteLine (failuresDir);
                Console.WriteLine (fixtureName);
                Console.WriteLine (failureFile);

                if (!Directory.Exists (failuresDir))
                    Directory.CreateDirectory (failuresDir);

                File.WriteAllText (failureFile, fixtureName);
            } else {
                Console.WriteLine ("Test passed.");
                if (File.Exists (failureFile))
                    File.Delete (failureFile);
            }
        }

        public string GetDevicePort ()
        {
            var devicePort = Environment.GetEnvironmentVariable ("MQTT_BRIDGE_EXAMPLE_DEVICE_PORT");

            if (String.IsNullOrEmpty (devicePort))
                devicePort = "/dev/ttyUSB0";

            Console.WriteLine ("Device port: " + devicePort);

            return devicePort;
        }

        public int GetDeviceSerialBaudRate ()
        {
            return 9600;
        }

        public void MoveToProjectDirectory ()
        {
            Directory.SetCurrentDirectory (ProjectDirectory);
        }

        public void MoveToTemporaryDirectory ()
        {
            var tmpDir = Path.Combine (ProjectDirectory, "_tmp");

            if (!Directory.Exists (tmpDir))
                Directory.CreateDirectory (tmpDir);

            var tmpTestDir = Path.Combine (tmpDir, Guid.NewGuid ().ToString ());

            if (!Directory.Exists (tmpTestDir))
                Directory.CreateDirectory (tmpTestDir);

            TemporaryDirectory = tmpTestDir;

            Directory.SetCurrentDirectory (tmpTestDir);
        }

        public void CleanTemporaryDirectory ()
        {
            var tmpDir = Environment.CurrentDirectory;

            Directory.SetCurrentDirectory (ProjectDirectory);

            Console.WriteLine ("Cleaning temporary directory:");
            Console.WriteLine (tmpDir);

            //Directory.Delete (tmpDir, true);
        }

        public DeviceInfo GetExampleDeviceInfo ()
        {
            return GetExampleDeviceInfo ("/dev/ttyUSB0");
        }

        public DeviceInfo GetExampleDeviceInfo (int portIndex)
        {
            return GetExampleDeviceInfo ("/dev/ttyUSB" + portIndex);
        }

        public DeviceInfo GetExampleDeviceInfo (string portName)
        {
            var info = new DeviceInfo ();

            info.FamilyName = "ExampleFamily";
            info.GroupName = "ExampleGroup";
            info.ProjectName = "ProjectName";
            info.BoardType = "uno";
            info.Port = portName;

            return info;
        }

        public DeviceInfo GetExampleDeviceInfo2 ()
        {
            return GetExampleDeviceInfo2 ("/dev/ttyUSB1");
        }

        public DeviceInfo GetExampleDeviceInfo2 (int portIndex)
        {
            return GetExampleDeviceInfo ("/dev/ttyUSB" + portIndex);
        }

        public DeviceInfo GetExampleDeviceInfo2 (string portName)
        {
            var info = new DeviceInfo ();

            info.FamilyName = "ExampleFamily2";
            info.GroupName = "ExampleGroup2";
            info.ProjectName = "ProjectName2";
            info.BoardType = "nano";
            info.Port = portName;

            return info;
        }

        public void PullFileFromProject (string fileName)
        {
            PullFileFromProject (fileName, false);
        }

        public void PullFileFromProject (string fileName, bool removeDestinationDirectory)
        {
            Console.WriteLine ("Pulling file from project:");
            Console.WriteLine ("  " + fileName);
            Console.WriteLine ("  Remove destination directory: " + removeDestinationDirectory);

            var sourceFile = Path.Combine (ProjectDirectory, fileName);

            if (!File.Exists (sourceFile))
                throw new FileNotFoundException ("File not found", sourceFile);

            var destinationFile = Path.Combine (TemporaryDirectory, fileName);

            if (removeDestinationDirectory) {
                var shortenedFileName = Path.GetFileName (fileName);
                destinationFile = Path.Combine (TemporaryDirectory, shortenedFileName);
            }

            var destinationDirectory = Path.GetDirectoryName (destinationFile);

            if (!Directory.Exists (destinationDirectory))
                Directory.CreateDirectory (destinationDirectory);

            File.Copy (sourceFile, destinationFile);

            Console.WriteLine ("");
        }

        public void CreateExampleDeviceInfoFiles ()
        {
            var info = GetExampleDeviceInfo ();

            var data = new DeviceInfoFileManager ();

            data.WriteInfoToFile (info);
        }

        public void CreateExampleDeviceInfoFiles (DeviceInfo info)
        {
            var data = new DeviceInfoFileManager ();

            data.WriteInfoToFile (info);
        }
    }
}
