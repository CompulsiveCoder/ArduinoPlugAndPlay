using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockBackgroundProcessStarter : BackgroundProcessStarter
    {
        public string LastCommandRun { get; set; }

        public bool DidStart { get; set; }

        public bool EnableCommandExecution { get; set; }

        public MockBackgroundProcessStarter ()
        {
        }

        public override Process Start (string command)
        {
            LastCommandRun = command;

            DidStart = true;

            if (EnableCommandExecution)
                return base.Start (command);
            else
                return null;
        }
    }
}

