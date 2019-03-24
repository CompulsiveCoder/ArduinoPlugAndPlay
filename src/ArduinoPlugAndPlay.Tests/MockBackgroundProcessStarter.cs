using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockBackgroundProcessStarter : BackgroundProcessStarter
    {
        public string LastCommandRun { get; set; }

        public bool DidStart { get; set; }

        public bool EnableCommandExecution { get; set; }

        public string Key { get; set; }

        public string Arguments { get; set; }

        public MockBackgroundProcessStarter ()
        {
        }

        public override Process Start (string key, string command, string arguments)
        {
            LastCommandRun = command + " " + arguments;

            DidStart = true;

            Key = key;
            Arguments = arguments;

            if (EnableCommandExecution)
                return base.Start (key, command, arguments);
            else
                return null;
        }
    }
}

