using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockProcessStarter : ProcessStarter
    {
        public string LastCommandRun { get; set; }

        public bool DidStart = false;

        public MockProcessStarter ()
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

