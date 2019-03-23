using System;
using System.Threading;
using System.Collections.Generic;
using duinocom;
using System.Text;
using System.IO;

namespace ArduinoPlugAndPlay
{
    public class DeviceManager
    {
        public ProcessStarter Starter = new ProcessStarter ();

        public DeviceInfoExtractor Extractor = new DeviceInfoExtractor ();

        public PlatformioWrapper Platformio = new PlatformioWrapper ();

        public DeviceInfoFileManager Data = new DeviceInfoFileManager ();

        public DeviceReaderWriter ReaderWriter = new DeviceReaderWriter ();

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

        public bool UseBashC = true;
        public bool UseCommandTimeout = true;

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

            CheckForNewDevices ();

            ProcessNewDevices ();

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
            
            if (NewDevicePorts.Count > 0) {
                Console.WriteLine ("");
                Console.WriteLine ("Adding new devices:");
                foreach (var newDevice in NewDevicePorts) {
                    AddDevice (newDevice);
                }
            }
            NewDevicePorts.Clear ();
        }

        public void AddDevice (string devicePort)
        {
            if (IsVerbose)
                Console.WriteLine ("Adding device: " + devicePort);
            
            if (!DevicePorts.Contains (devicePort)) {

                var info = ExtractDeviceInfo (devicePort);

                Console.WriteLine ("  " + devicePort);

                if (LaunchAddDeviceCommand (info)) {
                    DevicePorts.Add (devicePort);
                    Data.WriteInfoToFile (info);
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

            info.AddCommandCompleted = StartBashCommand (cmd);

            return info.AddCommandCompleted;
        }

        public bool LaunchRemoveDeviceCommand (DeviceInfo info)
        {
            var cmd = FixCommand (DeviceRemovedCommand, "remove", info);

            info.RemoveCommandCompleted = StartBashCommand (cmd);

            return info.RemoveCommandCompleted;
        }

        public string FixCommand (string initialCommand, string action, DeviceInfo info)
        {
            var fixedCommand = initialCommand;

            fixedCommand = InsertValues (fixedCommand, info);
            fixedCommand = fixedCommand + " > " + GetLogFile (action, info) + "";

            return fixedCommand;

        }

        #endregion

        public string GetLogFile (string action, DeviceInfo info)
        {
            var logsDir = Path.GetFullPath ("logs");
            if (!Directory.Exists (logsDir))
                Directory.CreateDirectory (logsDir);

            var dateString = DateTime.Now.ToString ().Replace ("/", "-").Replace (":", "-").Replace (" ", "-");

            var logFileName = action + "-" + info.GroupName + "-" + dateString + ".txt";

            var filePath = Path.Combine ("logs", logFileName);

            return filePath;
        }

        public bool StartBashCommand (string command)
        {
            Console.WriteLine ("");
            Console.WriteLine ("Starting BASH command:");
            Console.WriteLine (Environment.CurrentDirectory);
            var fullCommand = EscapeCharacters (command);

            if (UseBashC)
                fullCommand = "/bin/bash -c '" + EscapeCharacters (command) + "'";

            if (UseCommandTimeout)
                fullCommand = "timeout " + CommandTimeoutInSeconds + "s " + fullCommand;

            Console.WriteLine ("  " + fullCommand);

            Starter.Start (fullCommand);
            if (Starter.HasOutput) {
                Console.WriteLine ("Output:");
                Console.WriteLine (Starter.Output);
            }
            if (Starter.IsError)
                Console.WriteLine ("Error in BASH command!");
            else
                Console.WriteLine ("Finished BASH command!");
            Console.WriteLine ("");

            return !Starter.IsError;
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

            while (!allDetailsHaveBeenDetected) {
                var line = ReaderWriter.ReadLine ();
                if (!String.IsNullOrEmpty (line))
                    builder.AppendLine (line.Trim ());

                var output = builder.ToString ();

                allDetailsHaveBeenDetected = output.Contains (Extractor.FamilyNamePreText) &&
                output.Contains (Extractor.GroupNamePreText) &&
                output.Contains (Extractor.ProjectNamePreText) &&
                output.Contains (Extractor.BoardTypePreText);
            }

            var serialOutput = builder.ToString ();

            var info = Extractor.ExtractInfo (portName, serialOutput);

            ReaderWriter.Close ();

            return info;
        }
    }
}

