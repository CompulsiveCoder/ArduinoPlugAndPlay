using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockBackgroundProcessStarter : BackgroundProcessStarter
    {
        public string LastCommandRun { get; set; }

        public bool DidStart = false;

        public MockBackgroundProcessStarter ()
        {
        }

        public override Process Start (string command)
        {
            LastCommandRun = command;

            DidStart = true;

            return null;
        }
    }
}

