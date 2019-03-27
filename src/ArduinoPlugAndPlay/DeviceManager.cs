using System;
using System.Threading;
using System.Collections.Generic;
using duinocom;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace ArduinoPlugAndPlay
{
    public class DeviceManager
    {
        public ProcessStarter Starter = new ProcessStarter ();

        public BackgroundProcessStarter BackgroundStarter = new BackgroundProcessStarter ();

        public DeviceInfoExtractor Extractor = new DeviceInfoExtractor ();

        public PlatformioWrapper Platformio = new PlatformioWrapper ();

        public DeviceInfoFileManager Data = new DeviceInfoFileManager ();

        public DeviceReaderWriter ReaderWriter = new DeviceReaderWriter ();

        public TimeoutHelper Timeout = new TimeoutHelper ();

        public int SleepTimeInSeconds = 15;

        public bool IsActive = true;

        public string DeviceAddedCommand = "echo 'Device added ({FAMILY} {GROUP} {BOARD})'";
        public string DeviceRemovedCommand = "echo 'Device removed ({FAMILY} {GROUP} {BOARD})'";

        public List<string> DevicePorts = new List<string> ();
        public List<string> NewDevicePorts = new List<string> ();
        public List<string> RemovedDevicePorts = new List<string> ();

        public int DefaultBaudRate = 9600;

        public int DeviceInfoLineCount = 16;

        public bool IsVerbose = true;

        public int CommandTimeoutInSeconds = 5 * 60;
        public int TimeoutExtractingDetailsInSeconds = 1 * 60;

        public bool UseCommandTimeout = true;

        public int CommandRetryMax = 5;

        public DeviceManager ()
        {
            Starter.WriteOutputToConsole = true;
        }

        public void Run ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Running Arduino Plug and Play");
            Console.WriteLine ("");

            Console.WriteLine ("Settings");
            Console.WriteLine ("  Device added command:");
            Console.WriteLine ("    " + DeviceAddedCommand);
            Console.WriteLine ("  Device removed command:");
            Console.WriteLine ("    " + DeviceRemovedCommand);
            Console.WriteLine ("  Sleep time: " + SleepTimeInSeconds + " seconds between loops");
            Console.WriteLine ("");

            LoadExistingDeviceList ();

            while (IsActive) {
                try {
                    RunLoop ();
                } catch (Exception ex) {
                    Console.WriteLine ("==============================");
                    Console.WriteLine ("Error:");
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine ("==============================");
                }

                // Sleep for a while
                Thread.Sleep (SleepTimeInSeconds * 1000);
            }
        }

        public void RunLoop ()
        {
            Console.WriteLine ("----------");
            Console.WriteLine ("Starting DeviceManager loop...");
            Console.WriteLine ("");

            // Check devices
            CheckForNewDevices ();
            CheckForRemovedDevices ();

            // Clean up any problems
            CheckRunningProcesses ();

            // Handle device changes
            ProcessNewDevices ();
            ProcessRemovedDevices ();


            Console.WriteLine ("");
            Console.WriteLine ("Loop Completed!");
            Console.WriteLine ("----------");
            Console.WriteLine ("");
        }

        public void LoadExistingDeviceList ()
        {
            foreach (var device in Data.ReadAllDevicesFromFile()) {
                DevicePorts.Add (device.Port);
            }
        }

        #region New Devices

        public void CheckForNewDevices ()
        {
            if (IsVerbose)
                Console.WriteLine ("Checking for new devices...");

            var devicesAreConnected = AreDevicesConnected ();

            if (devicesAreConnected) {
                ProcessConnectedDevices ();
            } else
                Console.WriteLine ("No devices found.");
        }

        public void ProcessConnectedDevices ()
        {
            Console.WriteLine ("The following devices were detected:");

            var list = GetDeviceList ();

            foreach (var item in list) {

                var isNewDevice = !DevicePorts.Contains (item.Trim ());

                var deviceStatus = "";

                if (isNewDevice) {
                    deviceStatus = "new";
                    NewDevicePorts.Add (item);
                } else {
                    deviceStatus = "existing";
                }

                Console.WriteLine ("  " + item + " (" + deviceStatus + ")");
            }
        }

        public void ProcessNewDevices ()
        {
            if (IsVerbose)
                Console.WriteLine ("Processing new devices...");

            if (NewDevicePorts != null) {
                if (NewDevicePorts.Count > 0) {
                    Console.WriteLine ("");
                    Console.WriteLine ("Adding new devices:");

                    var nextDevicePort = NewDevicePorts [0];
                    if (nextDevicePort != null)
                        AddDevice (nextDevicePort);
                }
            }
        }

        public void AddDevice (string devicePort)
        {

            if (!Platformio.PortIsInList (devicePort)) {
                Console.WriteLine ("The device has been disconnected. Aborting.");
                NewDevicePorts.Remove (devicePort);
            } else {

                if (IsVerbose)
                    Console.WriteLine ("Adding device: " + devicePort);

                NewDevicePorts.Remove (devicePort);

                if (!DevicePorts.Contains (devicePort) && Platformio.PortIsInList (devicePort)) {
                    var info = ExtractDeviceInfo (devicePort);

                    Console.WriteLine ("  " + devicePort);

                    DevicePorts.Add (devicePort);
                    Data.WriteInfoToFile (info);

                    LaunchAddDeviceCommand (info);
                }
            }
        }

        #endregion

        #region Removed Devices

        public void CheckForRemovedDevices ()
        {
            if (IsVerbose)
                Console.WriteLine ("Checking for removed devices...");

            var existingDevices = Data.ReadAllDevicesFromFile ();

            if (existingDevices.Length > 0) {
                if (IsVerbose)
                    Console.WriteLine ("Existing devices (stored in file):");

                var detectedDevices = new List<string> (GetDeviceList ());

                foreach (var device in existingDevices) {

                    var deviceHasBeenRemoved = !detectedDevices.Contains (device.Port);

                    if (IsVerbose)
                        Console.WriteLine ("Has been removed: " + deviceHasBeenRemoved);
                
                    if (deviceHasBeenRemoved) {
                        RemovedDevicePorts.Add (device.Port);
                    }
                }
            }
        }

        public void ProcessRemovedDevices ()
        {
            if (RemovedDevicePorts.Count > 0) {
                Console.WriteLine ("");
                Console.WriteLine ("Removing devices:");

                while (RemovedDevicePorts.Count > 0) {
                    RemoveDevice (RemovedDevicePorts [0]);
                }
            }
        }

        public void RemoveDevice (string devicePort)
        {
            if (!String.IsNullOrEmpty (devicePort)) {
                Console.WriteLine ("  " + devicePort);

                var info = Data.ReadInfoFromFile (devicePort);

                Data.DeleteInfoFromFile (info.Port);

                DevicePorts.Remove (devicePort);

                RemovedDevicePorts.Remove (devicePort);

                LaunchRemoveDeviceCommand (info);

            }
        }

        #endregion

        #region Launch Commands

        public bool LaunchAddDeviceCommand (DeviceInfo info)
        {
            var action = "add";

            var cmd = FixCommand (DeviceAddedCommand, action, info);

            info.AddCommandCompleted = StartBashCommand (action, cmd, info);

            return info.AddCommandCompleted;
        }

        public bool LaunchRemoveDeviceCommand (DeviceInfo info)
        {
            var action = "remove";

            var cmd = FixCommand (DeviceRemovedCommand, action, info);

            info.RemoveCommandCompleted = StartBashCommand (action, cmd, info);

            return info.RemoveCommandCompleted;
        }

        public string FixCommand (string initialCommand, string action, DeviceInfo info)
        {
            var fixedCommand = initialCommand;

            fixedCommand = InsertValues (fixedCommand, info);
            fixedCommand = fixedCommand + " >> " + GetLogFile (action, info) + "";

            return fixedCommand;

        }

        #endregion

        public string GetLogFile (string action, DeviceInfo info)
        {
            var logsDir = Path.GetFullPath ("logs");
            if (!Directory.Exists (logsDir))
                Directory.CreateDirectory (logsDir);

            var date = DateTime.Now;

            var dateString = date.Year + "-" + date.Month + "-" + date.Day;

            var logFileName = dateString + "-" + info.Port.Replace ("/dev/", "") + "-" + info.GroupName + ".txt";

            var filePath = Path.Combine (logsDir, logFileName);

            return filePath;
        }

        public bool StartBashCommand (string action, string command, DeviceInfo info)
        {
            Console.WriteLine ("");
            Console.WriteLine ("Starting BASH command...");
            //Console.WriteLine (Environment.CurrentDirectory);

            var cmd = "/bin/bash";
            var arguments = "-c '" + EscapeCharacters (command) + "'";

            if (UseCommandTimeout) {
                arguments = CommandTimeoutInSeconds + "s " + cmd + " " + arguments;
                cmd = "timeout";
            }

            BackgroundStarter.Start (action, info, cmd, arguments);

            if (BackgroundStarter.IsError)
                Console.WriteLine ("Error in BASH command!");
            Console.WriteLine ("");

            return !BackgroundStarter.IsError;
        }

        public string EscapeCharacters (string startValue)
        {
            var newValue = startValue;
            newValue = newValue.Replace ("'", "\\'");

            return newValue.ToString ();
        }


        public bool AreDevicesConnected ()
        {
            return Platformio.AreDevicesDetected ();
        }

        public string[] GetDeviceList ()
        {
            var deviceList = Platformio.GetDeviceList ();

            return deviceList;
        }

        public string InsertValues (string startValue, DeviceInfo info)
        {
            if (IsVerbose) {
                Console.WriteLine ("Inserting values...");
                Console.WriteLine ("  Before:");
                Console.WriteLine ("    " + startValue);
            }
            var newValue = startValue;
            newValue = newValue.Replace ("{FAMILY}", info.FamilyName);
            newValue = newValue.Replace ("{GROUP}", info.GroupName);
            newValue = newValue.Replace ("{PROJECT}", info.ProjectName);
            newValue = newValue.Replace ("{BOARD}", info.BoardType);
            newValue = newValue.Replace ("{PORT}", info.Port.Replace ("/dev/", ""));

            if (IsVerbose) {
                Console.WriteLine ("  After:");
                Console.WriteLine ("    " + newValue);
            }
            return newValue;
        }

        public DeviceInfo ExtractDeviceInfo (string portName)
        {
            DeviceInfo info = null;

            if (!Platformio.PortIsInList (portName))
                Console.WriteLine ("The device has been disconnected. Aborting.");
            else {
                Console.WriteLine ("  Reading new device info from USB/serial output: " + portName);

                ReaderWriter.Open (portName, DefaultBaudRate);

                var builder = new StringBuilder ();

                var allDetailsHaveBeenDetected = false;

                Timeout.Start ();

                var deviceHasBeenDisconnected = false;

                // Send a command requesting the device info
                ReaderWriter.WriteLine ("#");

                while (!allDetailsHaveBeenDetected && !deviceHasBeenDisconnected) {
                    deviceHasBeenDisconnected = !Platformio.PortIsInList (portName);

                    var line = ReaderWriter.ReadLine ();
                    if (!String.IsNullOrEmpty (line))
                        builder.AppendLine (line.Trim ());

                    var output = builder.ToString ();

                    allDetailsHaveBeenDetected = output.Contains (Extractor.FamilyNamePreText) &&
                    output.Contains (Extractor.GroupNamePreText) &&
                    output.Contains (Extractor.ProjectNamePreText) &&
                    output.Contains (Extractor.BoardTypePreText);

                    Timeout.Check (TimeoutExtractingDetailsInSeconds * 1000, "Timed out attempting to read the details from the device.");
                }

                var serialOutput = builder.ToString ();

                info = Extractor.ExtractInfo (portName, serialOutput);

                ReaderWriter.Close ();
            }
            return info;
        }

        public void CheckRunningProcesses ()
        {
            if (BackgroundStarter.QueuedProcesses.Count > 0) {

                BackgroundStarter.EnsureProcessRunning ();

                Console.WriteLine ("");
                Console.WriteLine ("Checking status of existing processes...");

                var keys = new List<string> ();

                foreach (var item in BackgroundStarter.QueuedProcesses) {
                    keys.Add (item.Key);
                }

                Console.WriteLine ("  " + BackgroundStarter.QueuedProcesses.Count + " processes queued.");

                var processWrapper = BackgroundStarter.QueuedProcesses.Peek ();

                // If the process has completed
                if (processWrapper.HasExited) {
                    HandleProcessExit (processWrapper);
                } else if (processWrapper.HasStarted) { // Process is still running
                    CheckRunningProcess (processWrapper);
                }

                Console.WriteLine ("");
            }
        }

        public void CheckRunningProcess (ProcessWrapper processWrapper)
        { 
            Console.WriteLine ("  A process is running: " + processWrapper.Action + " " + processWrapper.Info.GroupName);

            CheckForAbortDueToDisconnect (processWrapper);
        }

        public void HandleProcessExit (ProcessWrapper processWrapper)
        {
            // Process failed
            if (processWrapper.Process.ExitCode != 0) {
                Console.WriteLine ("  A process failed.");
                ProcessFailure (processWrapper);
            } else { // Process succeeded
                Console.WriteLine ("  A process completed successfully.");
                BackgroundStarter.QueuedProcesses.Dequeue ();
            }
        }

        public ProcessWrapper GetQueuedProcessByKey (string key)
        {
            foreach (var process in BackgroundStarter.QueuedProcesses) {
                if (process.Key == key)
                    return process;
            }
            return null;
        }

        public void CheckForAbortDueToDisconnect (ProcessWrapper processWrapper)
        {
            var deviceHasBeenDisconnected = !DevicePorts.Contains (processWrapper.Info.Port)
                                            || RemovedDevicePorts.Contains (processWrapper.Info.Port)
                                            || !Platformio.PortIsInList (processWrapper.Info.Port);

            // If the device has been removed kill the process
            if (deviceHasBeenDisconnected && !processWrapper.HasExited) {
                AbortDueToDisconnect (processWrapper);
            }
        }

        public void AbortDueToDisconnect (ProcessWrapper processWrapper)
        {
            Console.WriteLine ("Aborting connect command due to device being disconnected while it's still running: " + processWrapper.Info.Port);
            processWrapper.Process.Kill ();
            BackgroundStarter.QueuedProcesses.Dequeue ();

            if (processWrapper.Action == "add") {
                Console.WriteLine ("  Launching remove command to clean up partial install.");
                LaunchRemoveDeviceCommand (processWrapper.Info);
            }
        }

        public void ProcessFailure (ProcessWrapper processWrapper)
        {
            if (processWrapper.TryCount >= CommandRetryMax) {
                WriteToLog (processWrapper, "Previous add command failed more than " + CommandRetryMax + " times. Aborting...");

                Console.WriteLine ("Process has been retried " + CommandRetryMax + ". Aborting.");
                BackgroundStarter.QueuedProcesses.Dequeue ();
                var info = Data.ReadInfoFromFile (processWrapper.Info.Port);

                // If the add command failed launch the remove command to clean up any
                // partially installed files
                if (processWrapper.Action == "add") {
                    WriteToLog (processWrapper, "Launching remove command to clean up failed install....");
                    LaunchRemoveDeviceCommand (info);
                }
            } else {
                WriteToLog (processWrapper, "Previous execution failed. Retrying....");

                processWrapper.IncrementTryCount ();

                Console.WriteLine ("Processing previous failure...");

                // Remove from front of the queue
                BackgroundStarter.QueuedProcesses.Dequeue ();

                // Mark the process wrapper as having not started yet so it can restart
                processWrapper.HasStarted = false;

                // Add to the back of the queue
                BackgroundStarter.QueuedProcesses.Enqueue (processWrapper);

                // Ensure that there's a process running.
                // This will restart the current one if its still at the front of the queue
                BackgroundStarter.EnsureProcessRunning ();

                Console.WriteLine ("  Failed process has been restarted.");
                Console.WriteLine ("");
            }
        }

        public void WriteToLog (ProcessWrapper processWrapper, string text)
        {
            var logFile = GetLogFile (processWrapper.Action, processWrapper.Info);
            File.AppendAllText (logFile, text + Environment.NewLine);
        }
    }
}

