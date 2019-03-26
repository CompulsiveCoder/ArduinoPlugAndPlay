using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace ArduinoPlugAndPlay
{
    public class BackgroundProcessStarter
    {
        public bool IsError { get; set; }

        public bool ThrowExceptionOnError = true;

        public bool WriteOutputToConsole = false;

        public bool HasOutput {
            get {
                return !String.IsNullOrWhiteSpace (Output); 
            }
        }

        public string Output { get; set; }

        public bool IsDebug = false;

        public Queue<ProcessWrapper> QueuedProcesses = new Queue<ProcessWrapper> ();

        public BackgroundProcessStarter ()
        {
        }

        /// <summary>
        /// Starts/executes a process in the current thread.
        /// </summary>
        /// <param name='command'></param>
        /// <param name='arguments'></param>
        public virtual Process Start (string action, DeviceInfo deviceInfo, string command, string arguments)
        {
            if (IsDebug) {
                Console.WriteLine ("");
                Console.WriteLine ("Starting process:");
                Console.WriteLine (command);
                Console.WriteLine ("");
            }

            // If the command has an extension (and is therefore an actual file)
            if (Path.GetExtension (command) != String.Empty) {
                // If the file doesn't exist
                if (!File.Exists (Path.GetFullPath (command)))
                    throw new ArgumentException ("Cannot find the file '" + Path.GetFullPath (command) + "'.");
            }

            // Create the process start information
            ProcessStartInfo processStartInfo = new ProcessStartInfo (
                                                    command,
                                                    arguments
                                                );

            // Configure the process
            processStartInfo.UseShellExecute = false;

            // Start the process
            Process process = new Process ();

            process.StartInfo = processStartInfo;

            var key = action + "-" + deviceInfo.Port;

            var processWrapper = new ProcessWrapper (action, deviceInfo, process);

            QueueProcess (key, processWrapper);

            EnsureProcessRunning ();

            return process;
        }

        public void QueueProcess (string key, ProcessWrapper processWrapper)
        {
            // Add the new process to the list
            QueuedProcesses.Enqueue (processWrapper);
        }

        public void EnsureProcessRunning ()
        {
            var isAProcessRunning = false;
            /*ProcessWrapper topProcessWrapper = null;

            // Figure out if a process is running yet
            foreach (var processWrapper in QueuedProcesses) {
                if (topProcessWrapper == null)
                    topProcessWrapper = processWrapper;


                if (processWrapper.HasStarted) {
                    if (processWrapper.HasExited)
                        isAProcessRunning = true;
                }
            }*/

            var processWrapper = QueuedProcesses.Peek ();

            // If the latest process isn't started then start it
            if (processWrapper != null && !processWrapper.HasStarted) {
                try {
                    Console.WriteLine ("Starting the next process in the queue: " + processWrapper.Action + " " + processWrapper.Info.GroupName);
                    processWrapper.Start ();
                } catch (Exception ex) {
                    IsError = true;

                    var title = "\"Error starting process.\"";

                    AppendOutputLine (title);
                    AppendOutputLine (ex.ToString ());

                    if (ThrowExceptionOnError)
                        throw new Exception (title, ex);
                    else {
                        Console.WriteLine ("");
                        Console.WriteLine (title);
                        Console.WriteLine (ex.ToString ());
                        Console.WriteLine ("");
                    }
                }

            }
        }

        public string[] FixArguments (string[] arguments)
        {
            List<string> argsList = new List<string> ();

            if (arguments != null && arguments.Length > 0)
                argsList.AddRange (arguments);

            for (int i = 0; i < argsList.Count; i++) {
                if (!String.IsNullOrEmpty (argsList [i])) {
                    argsList [i] = FixArgument (argsList [i]);
                }
            }

            return argsList.ToArray ();
        }

        public string FixArgument (string argument)
        {
            if (argument.Contains (" ")
                && argument.IndexOf ("\"") != 0)
                return @"""" + argument + @"""";
            else
                return argument;
        }

        public void ClearOutput ()
        {
            Output = String.Empty;
        }

        public void AppendOutput (string text)
        {
            Output += text;
        }

        public void AppendOutputLine (string line)
        {
            Output += line + Environment.NewLine;
        }
    }
}

