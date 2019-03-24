using System;
using System.Diagnostics;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockBackgroundProcessStarter : BackgroundProcessStarter
    {
        public Dictionary<string, string> CommandsRun = new Dictionary<string, string> ();

        public bool DidStart { get; set; }

        public bool EnableCommandExecution { get; set; }

        public MockBackgroundProcessStarter ()
        {
        }

        public override Process Start (string key, string command, string arguments)
        {
            var fullCommand = command + " " + arguments;

            CommandsRun.Add (key, fullCommand);

            DidStart = true;

            if (EnableCommandExecution)
                return base.Start (key, command, arguments);
            else
                return null;
        }
    }
}

