using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddDeviceRetryAfterErrorTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddDevice_RetryAfterError ()
        {
            var info = GetExampleDeviceInfo ();

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

            var createDeviceScript = @"

mkdir -p devices

DEVICE_NUMBER=1
DEVICE_NAME=""device$DEVICE_NUMBER""

while [ -f ""$DEVICE_NAME.txt"" ]; do
  echo ""Increasing device number""
  DEVICE_NUMBER=$((DEVICE_NUMBER+1))
  DEVICE_NAME=""device$DEVICE_NUMBER""
  echo ""Device name: $DEVICE_NAME""
done

echo ""$DEVICE_NAME"" > devices/$DEVICE_NAME.txt
";

            File.WriteAllText (Path.GetFullPath ("create-device.sh"), createDeviceScript);

            var removeDeviceScript = @"
rm ""devices/device1.txt""
";

            File.WriteAllText (Path.GetFullPath ("remove-device.sh"), removeDeviceScript);

            //deviceManager.USBDeviceConnectedCommand = "MAX=5; COUNT=0; [ -f \"fcf.txt\" ] && COUNT=\"$(cat fcf.txt)\"; COUNT=$(($COUNT+1)); echo \"$COUNT\" > \"fcf.txt\"; echo \"$COUNT\"; [ \"$COUNT\" -lt \"$MAX\" ] && echo \"Script intentionally failed\" && exit 1";
            deviceManager.USBDeviceConnectedCommand = "bash create-device.sh && echo \"Failed intentionally\" && exit 1";
            deviceManager.USBDeviceDisconnectedCommand = "bash remove-device.sh";

            var assertion = new AssertionHelper (deviceManager);

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (info));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (info.Port);

            Console.WriteLine ("");
            Console.WriteLine ("Looping through " + deviceManager.CommandRetryMax + " retries...");
            Console.WriteLine ("");

            var device1FilePath = Path.GetFullPath ("devices/device1.txt");
            var device2FilePath = Path.GetFullPath ("devices/device2.txt");

            for (int i = 0; i < deviceManager.CommandRetryMax - 1; i++) {
                Console.WriteLine ();
                Console.WriteLine ("--- Start Test Loop #" + (i + 1));
                Console.WriteLine ();

                Console.WriteLine ("Running a loop to execute the intentionally failing process...");
                deviceManager.RunLoop ();

                Console.WriteLine ("Sleeping to let the add device process finish...");
                Thread.Sleep (2000);

                Console.WriteLine ("Checking that device1.txt was created...");
                Console.WriteLine ("  " + device1FilePath);
                Assert.IsTrue (File.Exists (device1FilePath), "Dummy device file not found: device1.txt");

                Console.WriteLine ("Checking that device2.txt was NOT created...");
                Console.WriteLine ("  " + device2FilePath);
                Assert.IsFalse (File.Exists (device2FilePath), "Dummy device file found when it shouldn't have been created: device2.txt");

                Console.WriteLine ("Checking that 1 process (the add process) is in the queue...");
                Assert.AreEqual (1, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Wrong number of processes found.");

                Console.WriteLine ("Checking that the process in the queue is the add process...");
                var process = deviceManager.BackgroundStarter.QueuedProcesses.ToArray () [0];
                Assert.AreEqual ("add", process.Action, "The process in the queue isn't the add process");

                Console.WriteLine ("Running a loop to detect the process failure, launch the remove process, and then requeue the add process...");
                deviceManager.RunLoop ();

                Console.WriteLine ("Sleeping to let the remove process finish...");
                Thread.Sleep (3000);

                Console.WriteLine ("Checking that device1.txt was removed...");
                Assert.IsFalse (File.Exists (device1FilePath), "Dummy device file wasn't removed: device1.txt");

                Console.WriteLine ("Checking that the device info still exists...");
                assertion.AssertDeviceExists (info);

                Console.WriteLine ("Checking that both the remove and add devices are in the queue...");
                Assert.AreEqual (2, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Wrong number of processes found.");

                Console.WriteLine ("Checking that the first process in the queue is the remove command...");
                var newProcess1 = deviceManager.BackgroundStarter.QueuedProcesses.ToArray () [0];
                Assert.AreEqual ("remove", newProcess1.Action, "The first process in the queue isn't the remove process");

                Console.WriteLine ("Checking that the second process in the queue is the add command...");
                var newProcess2 = deviceManager.BackgroundStarter.QueuedProcesses.ToArray () [1];
                Assert.AreEqual ("add", newProcess2.Action, "The second process in the queue isn't the add process");

                Console.WriteLine ("Checking running processes, to dequeue the completed remove process...");
                deviceManager.CheckRunningProcesses ();

                Console.WriteLine ("Checking that device1.txt was removed...");
                Assert.IsFalse (File.Exists (device1FilePath), "Dummy device file wasn't removed: device1.txt");

                Console.WriteLine ("Checking that only one process (the add command) is in the queue...");
                Assert.AreEqual (1, deviceManager.BackgroundStarter.QueuedProcesses.Count, "Wrong number of processes found.");

                Console.WriteLine ("Checking that the process in the queue is the add command...");
                var restartedProcess1 = deviceManager.BackgroundStarter.QueuedProcesses.ToArray () [0];
                Assert.AreEqual ("add", restartedProcess1.Action, "The process in the queue isn't the add process");

                Console.WriteLine ();
                Console.WriteLine ("--- End Test Loop #" + i + 1);
                Console.WriteLine ();
            }

            Console.WriteLine ("Checking that device1.txt was removed...");
            Console.WriteLine ("  " + device1FilePath);
            Assert.IsFalse (File.Exists (device1FilePath), "Dummy device file was not removed: device1.txt");

            Console.WriteLine ("Checking that device2.txt was NOT created...");
            Console.WriteLine ("  " + device2FilePath);
            Assert.IsFalse (File.Exists (device2FilePath), "Dummy device file found when it shouldn't have been created: device2.txt");

            Console.WriteLine ("Running a loop so the add process can fail for the last time...");
            deviceManager.RunLoop ();

            Console.WriteLine ("Sleeping to let the add process finish...");
            Thread.Sleep (1000);

            Console.WriteLine ("Running a loop so the remove process can be started and the port will be added to the unusable ports list...");
            deviceManager.RunLoop ();

            Console.WriteLine ("Sleeping to let the remove process finish...");
            Thread.Sleep (3000);

            Console.WriteLine ("Checking that device1.txt was removed...");
            Console.WriteLine ("  " + device1FilePath);
            Assert.IsFalse (File.Exists (device1FilePath), "Dummy device file wasn't removed: device1.txt");

            Console.WriteLine ("Checking that device2.txt was NOT created...");
            Console.WriteLine ("  " + device2FilePath);
            Assert.IsFalse (File.Exists (device2FilePath), "Dummy device file found when it shouldn't have been created: device2.txt");

            Console.WriteLine ("Checking that the device port was added to the unusable ports list...");
            Assert.IsTrue (deviceManager.UnusableDevicePorts.Contains (info.Port), "Device port wasn't added to the unusable ports list");
        }
    }
}

