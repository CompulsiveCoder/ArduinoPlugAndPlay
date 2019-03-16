using System;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockProcessStarter : ProcessStarter
    {
        public string LastCommandRun { get; set; }

        public bool DidStart = false;

        public MockProcessStarter ()
        {
        }

        public override System.Diagnostics.Process Start (string command)
        {
            LastCommandRun = command;

            DidStart = true;

            return null;
        }
    }
}

