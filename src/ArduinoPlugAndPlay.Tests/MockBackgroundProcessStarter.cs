using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockBackgroundProcessStarter : BackgroundProcessStarter
    {
        public List<string> CommandsRun = new List<string> ();

        public bool DidStart { get; set; }

        public bool EnableCommandExecution { get; set; }

        public MockBackgroundProcessStarter ()
        {
        }

        public override Process Start (string action, DeviceInfo deviceInfo, string command, string arguments)
        {
            var fullCommand = command + " " + arguments;

            CommandsRun.Add (fullCommand);

            DidStart = true;

            if (EnableCommandExecution)
                return base.Start (action, deviceInfo, command, arguments);
            else
                return null;
        }
    }
}

