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

        public int SleepTimeInSeconds = 1;

        public bool IsActive = true;

        public string DeviceAddedCommand = "echo 'Device added ({FAMILY} {GROUP} {BOARD})'";
        public string DeviceRemovedCommand = "echo 'Device removed ({FAMILY} {GROUP} {BOARD})'";

        public List<string> DevicePorts = new List<string> ();
        public List<string> NewDevicePorts = new List<string> ();
        public List<string> RemovedDevicePorts = new List<string> ();

        public int DefaultBaudRate = 9600;

        public int DeviceInfoLineCount = 15;

        public bool IsVerbose = false;

        public int CommandTimeoutInSeconds = 10 * 60;
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
            Console.WriteLine ("---------------------------------");
            Console.WriteLine ("Starting DeviceManager loop...");
            Console.WriteLine ("");

            // Clean up any problems first
            CheckProcessStatus ();

            // Handle connected devices
            CheckForNewDevices ();
            ProcessNewDevices ();

            // Handle disconnected devices
            CheckForRemovedDevices ();
            ProcessRemovedDevices ();


            Console.WriteLine ("");
            Console.WriteLine ("Loop Completed!");
            Console.WriteLine ("---------------------------------");
            Console.WriteLine ("");
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

                var isNewDevice = !DevicePorts.Contains (item);

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

                    AddDevice (nextDevicePort);
                }
            }
        }

        public void AddDevice (string devicePort)
        {
            if (IsVerbose)
                Console.WriteLine ("Adding device: " + devicePort);
            
            if (!DevicePorts.Contains (devicePort)) {

                var info = ExtractDeviceInfo (devicePort);

                Console.WriteLine ("  " + devicePort);

                DevicePorts.Add (devicePort);
                Data.WriteInfoToFile (info);

                if (LaunchAddDeviceCommand (info)) {
                    NewDevicePorts.Remove (devicePort);
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

                if (LaunchRemoveDeviceCommand (info)) {
                    Data.DeleteInfoFromFile (info.Port);

                    DevicePorts.Remove (devicePort);

                    RemovedDevicePorts.Remove (devicePort);
                }
            }
        }

        #endregion

        #region Launch Commands

        public bool LaunchAddDeviceCommand (DeviceInfo info)
        {
            var cmd = FixCommand (DeviceAddedCommand, "add", info);

            info.AddCommandCompleted = StartBashCommand (cmd, info);

            return info.AddCommandCompleted;
        }

        public bool LaunchRemoveDeviceCommand (DeviceInfo info)
        {
            var cmd = FixCommand (DeviceRemovedCommand, "remove", info);

            info.RemoveCommandCompleted = StartBashCommand (cmd, info);

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

        public bool StartBashCommand (string command, DeviceInfo info)
        {
            Console.WriteLine ("");
            Console.WriteLine ("Starting BASH command:");
            //Console.WriteLine (Environment.CurrentDirectory);

            var cmd = "/bin/bash";
            var arguments = "-c '" + EscapeCharacters (command) + "'";

            if (UseCommandTimeout) {
                arguments = CommandTimeoutInSeconds + "s " + cmd + " " + arguments;
                cmd = "timeout";
            }

            BackgroundStarter.Start (info.Port, cmd, arguments);

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
            ReaderWriter.Open (portName, DefaultBaudRate);

            var builder = new StringBuilder ();

            var allDetailsHaveBeenDetected = false;

            Timeout.Start ();

            while (!allDetailsHaveBeenDetected) {
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

            var info = Extractor.ExtractInfo (portName, serialOutput);

            ReaderWriter.Close ();

            return info;
        }

        public void CheckProcessStatus ()
        {
            var totalRunningProcesses = 0;
            var totalFailedProcesses = 0;
            var totalSuccessfulProcesses = 0;

            var keys = new List<string> ();

            foreach (var item in BackgroundStarter.StartedProcesses) {
                keys.Add (item.Key);
            }

            foreach (var key in keys) {
                var process = BackgroundStarter.StartedProcesses [key];

                if (process.HasExited) {
                    if (process.ExitCode != 0) {
                        totalFailedProcesses++;
                        ProcessFailure (process);
                    } else {
                        totalSuccessfulProcesses++;
                        BackgroundStarter.StartedProcesses.Remove (key);
                    }
                } else {
                    totalRunningProcesses++;
                }
            }

            if (totalRunningProcesses > 0) {
                Console.WriteLine ("Processes running: " + totalRunningProcesses);
                Console.WriteLine ("");
            }
            if (totalFailedProcesses > 0) {
                Console.WriteLine ("Processes failed: " + totalFailedProcesses);
                Console.WriteLine ("");
            }
            if (totalSuccessfulProcesses > 0) {
                Console.WriteLine ("Processes successful: " + totalFailedProcesses);
                Console.WriteLine ("");
            }
        }

        public void ProcessFailure (Process process)
        {
            Console.WriteLine ("Processing previous failure...");
            process.Start ();
            Console.WriteLine ("  Failed process has been restarted.");
            Console.WriteLine ("");
        }
    }
}

