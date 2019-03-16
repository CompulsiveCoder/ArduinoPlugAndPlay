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

        public MockDeviceOutputs MockOutputs = new MockDeviceOutputs ();

        public BaseTestFixture ()
        {
        }

        [SetUp]
        public void Initialize ()
        {
            MoveToTemporaryDirectory ();

            Console.WriteLine ("");
            Console.WriteLine ("====================");
            Console.WriteLine ("Preparing test");

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

        public void MoveToTemporaryDirectory ()
        {
            ProjectDirectory = Environment.CurrentDirectory;

            var tmpDir = Path.GetFullPath (".tmp");

            if (!Directory.Exists (tmpDir))
                Directory.CreateDirectory (tmpDir);

            var tmpTestDir = Path.Combine (tmpDir, Guid.NewGuid ().ToString ());

            if (!Directory.Exists (tmpTestDir))
                Directory.CreateDirectory (tmpTestDir);

            Directory.SetCurrentDirectory (tmpTestDir);
        }

        public void CleanTemporaryDirectory ()
        {
            var tmpDir = Environment.CurrentDirectory;

            Directory.SetCurrentDirectory (ProjectDirectory);

            Console.WriteLine ("Cleaning temporary directory:");
            Console.WriteLine (tmpDir);

            Directory.Delete (tmpDir, true);
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
    }
}
