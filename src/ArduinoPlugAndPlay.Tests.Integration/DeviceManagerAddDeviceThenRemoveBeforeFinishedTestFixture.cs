﻿using System;
using NUnit.Framework;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    [TestFixture (Category = "Integration")]
    public class DeviceManagerAddDeviceThenRemoveBeforeFinishedTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_AddThenRemoveDeviceBeforeFinish ()
        {
            var deviceManager = new DeviceManager ();

            var mockPlatformio = new MockPlatformioWrapper ();
            var mockReaderWriter = new MockDeviceReaderWriter ();
            var mockBackgroundProcessStarter = new MockBackgroundProcessStarter ();

            deviceManager.Platformio = mockPlatformio;
            deviceManager.ReaderWriter = mockReaderWriter;
            deviceManager.BackgroundStarter = mockBackgroundProcessStarter;

            var assertion = new AssertionHelper (deviceManager);

            mockBackgroundProcessStarter.EnableCommandExecution = true;

            deviceManager.DeviceAddedCommand = "bash " + ProjectDirectory + "/count.sh";

            var deviceInfo = GetExampleDeviceInfo ();

            // Set the mock output from the device
            mockReaderWriter.SetMockOutput (MockOutputs.GetDeviceSerialOutput (deviceInfo));

            // Connect the virtual (mock) USB device
            mockPlatformio.ConnectDevice (deviceInfo.Port);

            // Run a loop to set off a long running command
            deviceManager.RunLoop ();

            // Give the command time to start
            Thread.Sleep (2000);

            // Disconnect the device before the command is completed
            mockPlatformio.DisconnectDevice (deviceInfo.Port);

            // Run another loop to see how it handles it
            deviceManager.RunLoop ();

            var logDir = Path.GetFullPath ("logs");
            Console.WriteLine (logDir);

            var logFilePath = "";
            foreach (var file in Directory.GetFiles(logDir, "*.txt"))
                logFilePath = file;

            Assert.IsNotNullOrEmpty (logFilePath, "Couldn't find log file.");

            var output = File.ReadAllText (logFilePath);

            Console.WriteLine ("Log file...");
            Console.WriteLine (output);
            Console.WriteLine ("");

            var reachableNumber = 1;

            Assert.IsTrue (output.Contains (reachableNumber.ToString ()), "Script continued past timeout.");

            var unreachableNumber = 5;

            Assert.IsFalse (output.Contains (unreachableNumber.ToString ()), "Script continued past timeout.");

            Assert.AreEqual (2, mockBackgroundProcessStarter.CommandsRun.Count, "Invalid number of commands run.");
        }

    }
}
