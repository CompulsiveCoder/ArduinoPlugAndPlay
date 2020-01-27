using System;
using System.Threading;
using System.Collections.Generic;
using duinocom;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;
using System.IO.Ports;
using System.Linq;

namespace ArduinoPlugAndPlay
{
    public class DeviceManager
    {
        public ProcessStarter Starter = new ProcessStarter ();
        public BackgroundProcessStarter BackgroundStarter = new BackgroundProcessStarter ();
        public DeviceInfoExtractor Extractor = new DeviceInfoExtractor ();
        // TODO: Remove if not needed. Should be obsolete.
        //public PlatformioWrapper Platformio = new PlatformioWrapper ();
        public DeviceInfoFileManager Data = new DeviceInfoFileManager ();
        public SerialDeviceReaderWriter ReaderWriter = new SerialDeviceReaderWriter ();
        public SerialPortWrapper SerialPort = new SerialPortWrapper ();
        public TimeoutHelper Timeout = new TimeoutHelper ();
        public int SleepTimeInSeconds = 1;
        public bool IsActive = true;
        public string USBDeviceConnectedCommand = "echo 'Device added ({FAMILY} {GROUP} {BOARD})'";
        public string USBDeviceDisconnectedCommand = "echo 'Device removed ({FAMILY} {GROUP} {BOARD})'";
        public string SmtpServer = String.Empty;
        public string EmailAddress = String.Empty;
        public List<string> DevicePorts = new List<string> ();
        public List<string> NewDevicePorts = new List<string> ();
        public List<string> RemovedDevicePorts = new List<string> ();
        public List<string> UnusableDevicePorts = new List<string> ();
        public string[] IgnoredSerialPorts = new string[] { };

        public int DefaultBaudRate = 9600;

        public bool IsVerbose = true;

        public int CommandTimeoutInSeconds = 5 * 60;
        public int TimeoutReadingDeviceInfoInSeconds = 30;

        public bool UseCommandTimeout = true;

        public int CommandRetryMax = 5;

        public string HostName = "";

        public DeviceManager ()
        {
            Starter.WriteOutputToConsole = true;
        }

        public void Run ()
        {
            Console.WriteLine ("");
            Console.WriteLine ("Running Arduino Plug and Play");
            Console.WriteLine ("");

            HostName = GetHostName ();

            Console.WriteLine ("Settings");
            Console.WriteLine ("  Device added command:");
            Console.WriteLine ("    " + USBDeviceConnectedCommand);
            Console.WriteLine ("  Device removed command:");
            Console.WriteLine ("    " + USBDeviceDisconnectedCommand);
            Console.WriteLine ("  Sleep time: " + SleepTimeInSeconds + " seconds between loops");
            Console.WriteLine ("  Host name: " + HostName);
            Console.WriteLine ("");

            LoadExistingDeviceListFromFiles ();

            while (IsActive) {
                try {
                    RunLoop ();
                } catch (TimeoutException ex) {
                    Console.WriteLine ("Timeout error:");
                    Console.WriteLine (ex.Message);

                    SendErrorEmail (ex);
                } catch (Exception ex) {
                    Console.WriteLine ("==============================");
                    Console.WriteLine ("Error:");
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine ("==============================");

                    SendErrorEmail (ex);
                }

                // Sleep for a while
                Thread.Sleep (SleepTimeInSeconds * 1000);
            }
        }

        public void RunLoop ()
        {
            var loopStartTime = DateTime.Now;

            Console.WriteLine ("----------");
            Console.WriteLine ("Starting Plug and Play loop...");
            Console.WriteLine ("");

            // Check running processes
            CheckRunningProcesses ();

            // Check for removed devices
            //if (BackgroundStarter.QueuedProcesses.Count == 0) {
            CheckForRemovedDevices ();
            ProcessRemovedDevices ();
            //}

            // Check for new devices
            if (BackgroundStarter.QueuedProcesses.Count == 0) {
                CheckForNewDevices ();
                ProcessNewDevices ();
            }

            var loopDuration = DateTime.Now.Subtract (loopStartTime);
            Console.WriteLine ("");
            Console.WriteLine ("  Loop duration: " + loopDuration.ToString ());
            Console.WriteLine ("Loop Completed!");
            Console.WriteLine ("----------");
            Console.WriteLine ("");
        }

        public void LoadExistingDeviceListFromFiles ()
        {
            Console.WriteLine ("Loading existing device list from files...");
            var startTime = DateTime.Now;
            foreach (var infoFromFile in Data.ReadAllDevicesFromFile()) {
                var infoFromDevice = ReadSerialDeviceInfo (infoFromFile.Port);

                Console.WriteLine ("  Device info found...");
                Console.WriteLine ("    Port: " + infoFromFile.Port);
                Console.WriteLine ("    Family: " + infoFromFile.FamilyName);
                Console.WriteLine ("    Project: " + infoFromFile.ProjectName);
                Console.WriteLine ("    Group: " + infoFromFile.GroupName);

                var filesMatchDeviceInfo = infoFromFile.DoesMatch (infoFromDevice);

                var physicalDeviceIsFound = infoFromDevice != null;

                if (!physicalDeviceIsFound) {
                    Console.WriteLine ("  Registered device is no longer connected to port. Removing: " + infoFromFile.Port);
                    RemoveDevice (infoFromFile.Port);
                } else if (!filesMatchDeviceInfo) {
                    Console.WriteLine ("  Registered device info doesn't match the info coming from the physical device: " + infoFromFile.Port);
                    HandlePortDeviceMismatch (infoFromFile.Port);
                } else {
                    Console.WriteLine ("  Registered device found is still connected. Adding to device ports list: " + infoFromFile.Port);
                    DevicePorts.Add (infoFromFile.Port);
                }
            }
            var duration = DateTime.Now.Subtract (startTime);
            Console.WriteLine ("  Duration: " + duration.ToString ());
            Console.WriteLine ("Finished loading existing device list from files.");
        }

        public void HandlePortDeviceMismatch (string portName)
        {
            Console.WriteLine ("Device on port " + portName + " has changed. Removing so it can be readded.");
            RemoveDevice (portName);
        }

        #region New Devices

        public void CheckForNewDevices ()
        {
            if (IsVerbose)
                Console.WriteLine ("Checking for new devices...");

            var list = GetDevicePortList ();

            var devicesHaveBeenDetected = false;
            if (list.Length > 0) {
                var titleHasBeenShown = false;
                foreach (var item in list) {
                    var isBlank = String.IsNullOrEmpty (item);
                    var isIgnored = IsPortIgnored (item.Trim ());
                    if (!isBlank && !isIgnored) {
                        devicesHaveBeenDetected = true;
                        if (!titleHasBeenShown) {
                            Console.WriteLine ("The following devices were detected:");
                            titleHasBeenShown = true;
                        }
                        var isNewDevice = !DevicePorts.Contains (item.Trim ());

                        var isUsableDevice = !UnusableDevicePorts.Contains (item.Trim ());

                        var deviceStatus = "";

                        if (isNewDevice && isUsableDevice) {
                            deviceStatus = "new";
                            NewDevicePorts.Add (item);
                        } else if (!isUsableDevice) {
                            deviceStatus = "unusable";
                        } else {
                            deviceStatus = "existing";
                        }

                        Console.WriteLine ("  " + item + " (" + deviceStatus + ")");
                    }
                }

            }

            if (!devicesHaveBeenDetected)
                Console.WriteLine ("  No devices detected.");
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
                    if (!String.IsNullOrEmpty (nextDevicePort))
                        AddDevice (nextDevicePort);
                }
            }
        }

        public void AddDevice (string devicePort)
        {
            if (String.IsNullOrEmpty (devicePort))
                throw new ArgumentException ("A device port must be provided.");

            if (IsVerbose)
                Console.WriteLine ("Adding device: " + devicePort);

            NewDevicePorts.Remove (devicePort);

            if (!DevicePorts.Contains (devicePort) && !BackgroundStarter.ProcessExists ("add", devicePort)) {
                // && Platformio.PortIsInList (devicePort)) // TODO: Check if this should be used. It's slow.
                var info = ReadSerialDeviceInfo (devicePort);

                DevicePorts.Add (devicePort);

                if (info != null) {
                    Console.WriteLine ("  " + devicePort);

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

            //var existingDevices = Data.ReadAllDevicesFromFile ();

            if (DevicePorts.Count > 0) {
                if (IsVerbose)
                    Console.WriteLine ("Existing devices (stored in file):");

                //var detectedDevices = new List<string> (GetDevicePortList ());

                for (int i = 0; i < DevicePorts.Count; i++) {
                    var portName = DevicePorts [i];

                    var deviceHasBeenRemoved = !SerialPort.GetPortNames ().Any (x => x == portName);
                    //var deviceHasBeenRemoved = !detectedDevices.Contains (portName);

                    if (IsVerbose)
                        Console.WriteLine ("Has been removed: " + deviceHasBeenRemoved);
                
                    if (deviceHasBeenRemoved) {
                        RemovedDevicePorts.Add (portName);
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
                    var port = RemovedDevicePorts [0];
                    if (!String.IsNullOrEmpty (port))
                        RemoveDevice (port);
                }
            }
        }

        public void HandlePortDisconnected (string devicePort)
        {
            RemoveDevice (devicePort);
        }

        public void RemoveDevice (string devicePort)
        {
            var devicePortIsEmpty = String.IsNullOrEmpty (devicePort);
            var removeProcessExists = BackgroundStarter.ProcessExists ("remove", devicePort);

            if (IsVerbose) {
                Console.WriteLine ("Removing device: " + devicePort);
                Console.WriteLine ("Device port name is empty: " + devicePortIsEmpty);
                Console.WriteLine ("Remove process exists: " + removeProcessExists);
            }
            if (!devicePortIsEmpty) {
                RemovedDevicePorts.Remove (devicePort);

                if (!removeProcessExists) {

                    DevicePorts.Remove (devicePort);
                
                    Console.WriteLine ("  " + devicePort);

                    if (UnusableDevicePorts.Contains (devicePort)) {
                        UnusableDevicePorts.Remove (devicePort);
                    } else {

                        var info = Data.ReadInfoFromFile (devicePort);

                        Data.DeleteInfoFromFile (info.Port);

                        LaunchRemoveDeviceCommand (info);
                    }
                } else {
                    if (IsVerbose)
                        Console.WriteLine ("Remove process already exists. Skipping remove.");
                }
            } else {
                if (IsVerbose)
                    Console.WriteLine ("Device port is empty. Skipping remove.");
            }
        }

        #endregion

        #region Launch Commands

        public bool LaunchAddDeviceCommand (DeviceInfo info)
        {
            if (String.IsNullOrEmpty (info.Port))
                throw new ArgumentException ("info.Port cannot be empty");

            var action = "add";

            var cmd = FixCommand (USBDeviceConnectedCommand, action, info);

            info.AddCommandCompleted = StartBashCommand (action, cmd, info);

            return info.AddCommandCompleted;
        }

        public bool LaunchRemoveDeviceCommand (DeviceInfo info)
        {
            if (String.IsNullOrEmpty (info.Port))
                throw new ArgumentException ("info.Port cannot be empty");

            var action = "remove";

            var cmd = FixCommand (USBDeviceDisconnectedCommand, action, info);

            info.RemoveCommandCompleted = StartBashCommand (action, cmd, info);

            return info.RemoveCommandCompleted;
        }

        public string FixCommand (string initialCommand, string action, DeviceInfo info)
        {
            var fixedCommand = initialCommand;

            fixedCommand = InsertValues (fixedCommand, info);
            fixedCommand = fixedCommand + " >> " + GetLogFile (info) + "";

            return fixedCommand;

        }

        #endregion

        public string GetLogFile (DeviceInfo deviceInfo)
        {
            var logsDir = Path.GetFullPath ("logs");
            if (!Directory.Exists (logsDir))
                Directory.CreateDirectory (logsDir);

            var date = DateTime.Now;

            var dateString = date.Year + "-" + date.Month + "-" + date.Day + "-" + date.Hour + "-" + date.Minute;

            var logFileName = dateString + "-" + deviceInfo.Port.Replace ("/dev/", "") + "-" + deviceInfo.GroupName + "-" + deviceInfo.ProjectName + "-" + deviceInfo.BoardType + ".txt";

            var filePath = Path.Combine (logsDir, logFileName);

            return filePath;
        }

        public string GetLogFile (string portName)
        {
            var logsDir = Path.GetFullPath ("logs");
            if (!Directory.Exists (logsDir))
                Directory.CreateDirectory (logsDir);

            var date = DateTime.Now;

            var dateString = date.Year + "-" + date.Month + "-" + date.Day;

            var logFileNameSection = dateString + "-" + portName.Replace ("/dev/", "");

            var logFilePath = String.Empty;

            foreach (var filePath in Directory.GetFiles(Path.GetFullPath("logs"))) {
                var fileName = Path.GetFileName (filePath);
                if (fileName.Contains (logFileNameSection))
                    logFilePath = filePath;
            }

            return logFilePath;
        }

        public string ReadAllLogFiles ()
        {
            var builder = new StringBuilder ();
            if (Directory.Exists (Path.GetFullPath ("logs"))) {
                foreach (var file in Directory.GetFiles(Path.GetFullPath("logs"))) {
                    var content = File.ReadAllText (file);
                    builder.AppendLine (Path.GetFileName (file));
                    builder.AppendLine ("");
                    builder.AppendLine (content);
                    builder.AppendLine ("");
                    builder.AppendLine ("");
                }
            }
            return builder.ToString ();
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
            return GetDevicePortList ().Length > 0;
            // TODO: Remove if not needed
            //return Platformio.AreDevicesDetected ();
        }

        public string[] GetDevicePortList ()
        {
            var deviceList = SerialPort.GetPortNames ();
            // TODO: Remove if not needed
            //var deviceList = Platformio.GetDeviceList ();

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
            newValue = newValue.Replace ("{DEVICENAME}", info.DeviceName);
            newValue = newValue.Replace ("{BOARD}", info.BoardType);
            newValue = newValue.Replace ("{SCRIPTCODE}", info.ScriptCode);
            newValue = newValue.Replace ("{PORT}", info.Port.Replace ("/dev/", ""));

            if (IsVerbose) {
                Console.WriteLine ("  After:");
                Console.WriteLine ("    " + newValue);
            }
            return newValue;
        }

        public DeviceInfo ReadSerialDeviceInfo (string portName)
        {
            DeviceInfo info = null;

            Console.WriteLine ("  Reading new device info from USB/serial output: " + portName);

            var extractionStartTime = DateTime.Now;

            try {
                ReaderWriter.Open (portName, DefaultBaudRate);

                var builder = new StringBuilder ();

                var allDetailsHaveBeenDetected = false;

                // Read the first line from the device before sending a command.
                // This seems to allow commands to function properly
                ReaderWriter.ReadLine ();

                ReaderWriter.WriteLine ("#");

                var i = 0;

                Console.WriteLine ("");

                Timeout.Start ();

                while (!allDetailsHaveBeenDetected) {
                    // Resend the # command after every 10 lines if the data hasn't been received
                    var modValue = i % 15;
                    if (i == 0 || modValue == 0) {
                        ReaderWriter.WriteLine ("#");
                    }

                    var line = ReaderWriter.ReadLine ();
                    if (!String.IsNullOrEmpty (line)) {
                        Console.WriteLine ("> " + line);
                        builder.AppendLine (line.Trim ());
                    }

                    var output = builder.ToString ();

                    allDetailsHaveBeenDetected = output.Contains (Extractor.EndDeviceInfoText);

                    Timeout.Check (TimeoutReadingDeviceInfoInSeconds * 1000, "Timed out attempting to read the details from the device.");

                    i++;
                }

                Console.WriteLine ("");

                var serialOutput = builder.ToString ();

                info = Extractor.ExtractInfo (portName, serialOutput);

                ReaderWriter.Close ();

                var extractionDuration = DateTime.Now.Subtract (extractionStartTime);

                Console.WriteLine ("  Device info extraction duration: " + extractionDuration.ToString ());
            } catch (TimeoutException) {
                Console.WriteLine ("Timed out. Aborting.");

                if (!UnusableDevicePorts.Contains (portName))
                    UnusableDevicePorts.Add (portName);
            } catch (IOException ex) {
                var reportError = false;
                var deviceIsUnusable = false;

                if (ex.Message.Contains ("Input/output error")
                    || ex.Message.Contains ("No such file or directory")) {
                    Console.WriteLine ("Device was likely disconnected. Aborting install.");
                } else if (ex.Message.Contains ("Invalid argument")) {
                    Console.WriteLine ("Error: Invalid argument");
                    Console.WriteLine ("Device is unusable.");
                    deviceIsUnusable = true;
                } else if (ex.Message.Contains ("Inappropriate ioctl for device")) {
                    Console.WriteLine ("Error: Inappropriate ioctl for device");
                    Console.WriteLine ("Device is unusable.");
                    deviceIsUnusable = true;
                } else {
                    Console.WriteLine ("An error occurred. Aborting install.");
                    reportError = true;
                    deviceIsUnusable = true;
                }

                if (deviceIsUnusable) {
                    if (!UnusableDevicePorts.Contains (portName))
                        UnusableDevicePorts.Add (portName);
                }

                if (reportError) {
                    Console.WriteLine ("An error occurred.");
                    Console.WriteLine (ex.ToString ());

                    SendErrorEmail (ex, portName);
                }
            }
            return info;
        }

        public void CheckRunningProcesses ()
        {
            if (BackgroundStarter.QueuedProcesses.Count > 0) {

                var latestProcess = BackgroundStarter.QueuedProcesses.Peek ();
                if (RemovedDevicePorts.Contains (latestProcess.Info.Port)) {
                    Console.WriteLine ("Device was removed. Aborting install process.");
                    BackgroundStarter.QueuedProcesses.Dequeue ();
                    RemoveDevice (latestProcess.Info.Port);
                } else {

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
        }

        public void CheckRunningProcess (ProcessWrapper processWrapper)
        { 
            Console.WriteLine ("  A process is running: " + processWrapper.Action + " " + processWrapper.Info.ProjectName + " " + processWrapper.Info.DeviceName);
            Console.WriteLine ("    Start time: " + processWrapper.StartTime.ToString ());
            Console.WriteLine ("    Duration: " + processWrapper.Duration.ToString ());

            CheckForAbortDueToDisconnect (processWrapper);
        }

        public void HandleProcessExit (ProcessWrapper processWrapper)
        {
            // Process failed
            if (processWrapper.Process.ExitCode != 0) {
                Console.WriteLine ("  A process failed: " + processWrapper.Action + " " + processWrapper.Info.ProjectName + " " + processWrapper.Info.DeviceName);
                Console.WriteLine ("    Start time: " + processWrapper.StartTime.ToString ());
                Console.WriteLine ("    Duration: " + processWrapper.Duration.ToString ());
                ProcessFailure (processWrapper);
            } else { // Process succeeded
                Console.WriteLine ("  A process completed successfully: " + processWrapper.Action + " " + processWrapper.Info.ProjectName + " " + processWrapper.Info.DeviceName);
                Console.WriteLine ("    Start time: " + processWrapper.StartTime.ToString ());
                Console.WriteLine ("    Duration: " + processWrapper.Duration.ToString ());
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
                                            || RemovedDevicePorts.Contains (processWrapper.Info.Port);

            // If the device has been removed kill the process
            if (deviceHasBeenDisconnected && processWrapper.Action == "add" && !processWrapper.HasExited) {
                AbortDueToDisconnect (processWrapper);
            }
        }

        public void AbortDueToDisconnect (ProcessWrapper processWrapper)
        {
            Console.WriteLine ("Aborting connect command due to device being disconnected while it's still running: " + processWrapper.Action + " " + processWrapper.Info.ProjectName + " " + processWrapper.Info.DeviceName);
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

                // Run the remove device command before restarting the process
                if (processWrapper.Action == "add")
                    LaunchRemoveDeviceCommand (processWrapper.Info);

                // If the process hasn't reached the maximum number of retries
                if (processWrapper.TryCount < CommandRetryMax) {
                    // Relaunch the add device command at the back of the queue
                    BackgroundStarter.QueuedProcesses.Enqueue (processWrapper);
                } else {
                    if (processWrapper.Action == "add") {
                        // Add the port to the unusable ports list
                        UnusableDevicePorts.Add (processWrapper.Info.Port);
                    }
                }

                // Ensure that there's a process running.
                // This will restart the current one if its still at the front of the queue
                BackgroundStarter.EnsureProcessRunning ();

                Console.WriteLine ("  Failed process has been restarted.");
                Console.WriteLine ("");
            }
        }

        public void WriteToLog (ProcessWrapper processWrapper, string text)
        {
            var logFile = GetLogFile (processWrapper.Info);
            File.AppendAllText (logFile, text + Environment.NewLine);
        }

        public void SendErrorEmail (Exception error)
        {
            
            var message = "The following error was thrown ArduinoPlugAndPlay utility...\n\n" + error.ToString ();

            var logs = ReadAllLogFiles ();

            message += "\n\nLog files...\n\n" + logs;

            SendErrorEmail (message);
            
        }

        public void SendErrorEmail (Exception error, string portName)
        {
            var message = "The following error was thrown by ArduinoPlugAndPlay utility on " + HostName + "...\n\nPort: " + portName + "\n\n" + error.ToString ();

            var logFile = GetLogFile (portName);

            if (!String.IsNullOrEmpty (logFile)) {
                var logFileContent = File.ReadAllText (logFile);

                message += "\n\nLog file...\n\n" + logFileContent;
            }

            SendErrorEmail (message);
        }

        public void SendErrorEmail (string message)
        {
            var areDetailsProvided = (SmtpServer != "mail.example.com" &&
                                     EmailAddress != "user@example.com" &&
                                     SmtpServer.ToLower () != "na" &&
                                     EmailAddress.ToLower () != "na" &&
                                     !String.IsNullOrWhiteSpace (SmtpServer) &&
                                     !String.IsNullOrWhiteSpace (EmailAddress));

            if (areDetailsProvided) {
                try {
                    var subject = "Error: ArduinoPlugAndPlay on " + HostName;
                    var body = message;

                    var mail = new MailMessage (EmailAddress, EmailAddress, subject, body);

                    var smtpClient = new SmtpClient (SmtpServer);

                    smtpClient.Send (mail);

                } catch (Exception ex) {
                    Console.WriteLine ("");
                    Console.WriteLine ("An error occurred while sending error report...");
                    Console.WriteLine ("SMTP Server: " + SmtpServer);
                    Console.WriteLine ("Email Address: " + EmailAddress);
                    Console.WriteLine ("");
                    Console.WriteLine (ex.ToString ());
                    Console.WriteLine ("");
                }
            } else {
                Console.WriteLine ("");
                Console.WriteLine ("SMTP server and email address not provided. Skipping error report email.");
                Console.WriteLine ("");
            }
        }

        public static string GetHostName ()
        {
            var starter = new ProcessStarter ();

            starter.WriteOutputToConsole = false;

            starter.Start ("hostname");

            var selfHostName = "";
            if (!starter.IsError) {
                selfHostName = starter.Output.Trim ();
            }

            return selfHostName;
        }

        public bool IsPortIgnored (string portName)
        {
            return Array.IndexOf (IgnoredSerialPorts, portName) > -1;
        }
    }
}

