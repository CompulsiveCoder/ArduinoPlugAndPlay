using System;
using System.Threading;
using System.Collections.Generic;
using duinocom;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;

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

        public int SleepTimeInSeconds = 5;

        public bool IsActive = true;

        public string DeviceAddedCommand = "echo 'Device added ({FAMILY} {GROUP} {BOARD})'";
        public string DeviceRemovedCommand = "echo 'Device removed ({FAMILY} {GROUP} {BOARD})'";

        public string SmtpServer = String.Empty;
        public string EmailAddress = String.Empty;

        public List<string> DevicePorts = new List<string> ();
        public List<string> NewDevicePorts = new List<string> ();
        public List<string> RemovedDevicePorts = new List<string> ();
        public List<string> UnusableDevicePorts = new List<string> ();

        public int DefaultBaudRate = 9600;

        public int DeviceInfoLineCount = 16;

        public bool IsVerbose = true;

        public int CommandTimeoutInSeconds = 5 * 60;
        public int TimeoutExtractingDetailsInSeconds = 30;

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
            Console.WriteLine ("----------");
            Console.WriteLine ("Starting Plug and Play loop...");
            Console.WriteLine ("");

            // Check devices
            CheckForRemovedDevices ();
            CheckForNewDevices ();

            // Check existing processes
            CheckRunningProcesses ();

            // Handle device changes
            ProcessRemovedDevices ();
            ProcessNewDevices ();

            Console.WriteLine ("");
            Console.WriteLine ("Loop Completed!");
            Console.WriteLine ("----------");
            Console.WriteLine ("");
        }

        public void LoadExistingDeviceList ()
        {
            Console.WriteLine ("Loading existing device list from files...");
            foreach (var infoFromFile in Data.ReadAllDevicesFromFile()) {
                var infoFromDevice = ExtractDeviceInfo (infoFromFile.Port);
                var filesMatchDeviceInfo = infoFromFile.DoesMatch (infoFromDevice);

                var physicalDeviceIsFound = infoFromDevice != null;

                if (!physicalDeviceIsFound)
                    RemoveDevice (infoFromFile.Port);
                else if (!filesMatchDeviceInfo)
                    HandlePortDeviceMismatch (infoFromFile.Port);
                else
                    DevicePorts.Add (infoFromFile.Port);
            }
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
            // TODO: Remove if not needed. This check is slow.
            /*if (!Platformio.PortIsInList (devicePort)) {
                Console.WriteLine ("The device has been disconnected. Aborting.");
                NewDevicePorts.Remove (devicePort);
            } else {*/

            if (IsVerbose)
                Console.WriteLine ("Adding device: " + devicePort);

            NewDevicePorts.Remove (devicePort);

            if (!DevicePorts.Contains (devicePort)) {
                // && Platformio.PortIsInList (devicePort)) // TODO: Check if this should be used. It's slow.
                var info = ExtractDeviceInfo (devicePort);

                DevicePorts.Add (devicePort);

                if (info != null) {
                    Console.WriteLine ("  " + devicePort);

                    Data.WriteInfoToFile (info);

                    LaunchAddDeviceCommand (info);
                }
            }
            // }
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

                var detectedDevices = new List<string> (GetDeviceList ());

                for (int i = 0; i < DevicePorts.Count; i++) {
                    var portName = DevicePorts [i];
                    var deviceHasBeenRemoved = !detectedDevices.Contains (portName);

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
                    RemoveDevice (RemovedDevicePorts [0]);
                }
            }
        }

        public void HandlePortDisconnected (string devicePort)
        {
            RemoveDevice (devicePort);
        }

        public void RemoveDevice (string devicePort)
        {
            if (!String.IsNullOrEmpty (devicePort)) {

                DevicePorts.Remove (devicePort);

                RemovedDevicePorts.Remove (devicePort);
                
                Console.WriteLine ("  " + devicePort);

                if (UnusableDevicePorts.Contains (devicePort)) {
                    UnusableDevicePorts.Remove (devicePort);
                } else {

                    var info = Data.ReadInfoFromFile (devicePort);

                    Data.DeleteInfoFromFile (info.Port);

                    LaunchRemoveDeviceCommand (info);
                }

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
            fixedCommand = fixedCommand + " >> " + GetLogFile (info.Port, info.GroupName) + "";

            return fixedCommand;

        }

        #endregion

        public string GetLogFile (string portName, string groupName)
        {
            var logsDir = Path.GetFullPath ("logs");
            if (!Directory.Exists (logsDir))
                Directory.CreateDirectory (logsDir);

            var date = DateTime.Now;

            var dateString = date.Year + "-" + date.Month + "-" + date.Day;

            var logFileName = dateString + "-" + portName.Replace ("/dev/", "") + "-" + groupName + ".txt";

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
            foreach (var file in Directory.GetFiles(Path.GetFullPath("logs"))) {
                var content = File.ReadAllText (file);
                builder.AppendLine (Path.GetFileName (file));
                builder.AppendLine ("");
                builder.AppendLine (content);
                builder.AppendLine ("");
                builder.AppendLine ("");
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

            // TODO: Remove if not needed. This check is slow
            //if (!Platformio.PortIsInList (portName))
            //    Console.WriteLine ("The device has been disconnected. Aborting.");
            //else {
            Console.WriteLine ("  Reading new device info from USB/serial output: " + portName);

            try {
                ReaderWriter.Open (portName, DefaultBaudRate);

                var builder = new StringBuilder ();

                var allDetailsHaveBeenDetected = false;

                var deviceHasBeenDisconnected = false;

                Thread.Sleep (1000);


                // Read the first line from the device before sending a command.
                // This seems to allow commands to function properly
                ReaderWriter.ReadLine ();

                ReaderWriter.WriteLine ("#");

                var i = 0;

                Console.WriteLine ("");

                Timeout.Start ();

                while (!allDetailsHaveBeenDetected) {
                    // Resend the # command after every 10 lines if the data hasn't been received
                    var modValue = i % 10;
                    if (i == 0 || modValue == 0) {
                        ReaderWriter.WriteLine ("#");
                    }

                    var line = ReaderWriter.ReadLine ();
                    if (!String.IsNullOrEmpty (line)) {
                        Console.WriteLine ("> " + line);
                        builder.AppendLine (line.Trim ());
                    }

                    var output = builder.ToString ();

                    allDetailsHaveBeenDetected = output.Contains (Extractor.FamilyNamePreText) &&
                    output.Contains (Extractor.GroupNamePreText) &&
                    output.Contains (Extractor.ProjectNamePreText) &&
                    output.Contains (Extractor.BoardTypePreText);

                    Timeout.Check (TimeoutExtractingDetailsInSeconds * 1000, "Timed out attempting to read the details from the device.");

                    i++;
                }

                Console.WriteLine ("");

                var serialOutput = builder.ToString ();

                info = Extractor.ExtractInfo (portName, serialOutput);

                ReaderWriter.Close ();
                //}
            } catch (TimeoutException ex) {
                Console.WriteLine ("Timed out. Aborting.");

                if (!UnusableDevicePorts.Contains (portName))
                    UnusableDevicePorts.Add (portName);
            } catch (IOException ex) {
                if (ex.Message.Contains ("Input/output error")
                    || ex.Message.Contains ("No such file or directory")) {
                    Console.WriteLine ("Device was likely disconnected. Aborting install.");
                } else {
                    Console.WriteLine ("An error occurred. The device may have been disconnected. Aborting install.");
                    Console.WriteLine (ex.ToString ());

                    SendErrorEmail (ex, portName);
                }
            } catch (Exception ex) {
                if (!UnusableDevicePorts.Contains (portName))
                    UnusableDevicePorts.Add (portName);

                Console.WriteLine ("An error occurred.");
                Console.WriteLine (ex.ToString ());

                SendErrorEmail (ex, portName);
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
            Console.WriteLine ("  A process is running: " + processWrapper.Action + " " + processWrapper.Info.GroupName);
            Console.WriteLine ("    Duration: " + processWrapper.Duration.ToString ());

            CheckForAbortDueToDisconnect (processWrapper);
        }

        public void HandleProcessExit (ProcessWrapper processWrapper)
        {
            // Process failed
            if (processWrapper.Process.ExitCode != 0) {
                Console.WriteLine ("  A process failed: " + processWrapper.Action + " " + processWrapper.Info.GroupName);
                Console.WriteLine ("    Duration: " + processWrapper.Duration.ToString ());
                ProcessFailure (processWrapper);
            } else { // Process succeeded
                Console.WriteLine ("  A process completed successfully: " + processWrapper.Action + " " + processWrapper.Info.GroupName);
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
                                            || RemovedDevicePorts.Contains (processWrapper.Info.Port)
                                            || !Platformio.PortIsInList (processWrapper.Info.Port);

            // If the device has been removed kill the process
            if (deviceHasBeenDisconnected && !processWrapper.HasExited) {
                AbortDueToDisconnect (processWrapper);
            }
        }

        public void AbortDueToDisconnect (ProcessWrapper processWrapper)
        {
            Console.WriteLine ("Aborting connect command due to device being disconnected while it's still running: " + processWrapper.Action + " " + processWrapper.Info.GroupName);
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
            var logFile = GetLogFile (processWrapper.Info.Port, processWrapper.Info.GroupName);
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
            
            var message = "The following error was thrown ArduinoPlugAndPlay utility...\n\nPort: " + portName + "\n\n" + error.ToString ();

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
                    var subject = "Error: ArduinoPlugAndPlay";
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
    }
}

