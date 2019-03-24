using System;
using System.Text;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;

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

        public Dictionary<string, Process> StartedProcesses = new Dictionary<string, Process> ();

        public BackgroundProcessStarter ()
        {
        }

        /// <summary>
        /// Starts/executes a process in the current thread.
        /// </summary>
        /// <param name='command'></param>
        /// <param name='arguments'></param>
        public virtual Process Start (string key, string command, string arguments)
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
            ProcessStartInfo info = new ProcessStartInfo (
                                        command,
                                        arguments
                                    );

            // Configure the process
            info.UseShellExecute = false;

            // Start the process
            Process process = new Process ();

            process.StartInfo = info;

            // If an existing process is running kill it and remove it
            if (StartedProcesses.ContainsKey (key)) {
                StartedProcesses [key].Kill ();
                StartedProcesses.Remove (key);
            }
            // Add the new process to the list
            StartedProcesses.Add (key, process);

            try {
                process.Start ();
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

            return process;
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

