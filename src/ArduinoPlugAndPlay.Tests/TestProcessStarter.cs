using System;
using System.IO;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests
{
    public class TestProcessStarter
    {
        public BackgroundProcessStarter Starter = new BackgroundProcessStarter ();

        public string WorkingDirectory = Directory.GetCurrentDirectory ();

        public string PreCommand = "sh clean.sh";

        public bool IsMockSystemCTL = true;
        public bool IsMockHardware = true;
        public bool IsMockMqttBridge = true;

        public TestProcessStarter ()
        {
            //Starter.IsVerbose = false;
        }

        public void Initialize ()
        {
            Console.WriteLine ("Initializing the test");

            RunProcess (PreCommand);

            if (IsMockHardware)
                File.WriteAllText (Path.GetFullPath ("is-mock-hardware.txt"), 1.ToString ());
            else
                File.Delete (Path.GetFullPath ("is-mock-hardware.txt"));

            if (IsMockSystemCTL)
                File.WriteAllText (Path.GetFullPath ("is-mock-systemctl.txt"), 1.ToString ());
            else
                File.Delete (Path.GetFullPath ("is-mock-systemctl.txt"));
        }

        protected string RunProcess (string command)
        {
            var currentDirectory = Environment.CurrentDirectory;

            Directory.SetCurrentDirectory (WorkingDirectory);

            Console.WriteLine ("Running process...");
            Console.WriteLine (command);

            var finishedCommand = "/bin/bash -c '" + command + "'";

            Starter.Start (finishedCommand);
            var output = Starter.Output;

            Directory.SetCurrentDirectory (currentDirectory);

            return output;
        }

        public string RunBash (string internalCommand)
        {
            // Console.WriteLine ("Running bash command: ");
            // Console.WriteLine (internalCommand);

            var output = String.Empty;

            output += RunProcess (internalCommand);

            return output;
        }

        public string RunScript (string scriptName)
        {
            if (!scriptName.EndsWith (".sh"))
                scriptName += ".sh";

            var fullCommand = "sh " + scriptName;

            return RunBash (fullCommand);
        }

    }
}
