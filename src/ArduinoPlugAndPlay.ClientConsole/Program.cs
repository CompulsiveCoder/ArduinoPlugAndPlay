using System;
using duinocom;
using System.Threading;
using System.Text;
using System.Configuration;
using System.IO;
using System.IO.Ports;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Runtime;
using System.Diagnostics;

namespace ArduinoPlugAndPlay.ClientConsole
{
    class MainClass
    {
        public static bool IsVerbose = false;

        public static void Main (string[] args)
        {
            var arguments = new Arguments (args);

            Run (arguments);
        }

        public static void Run (Arguments arguments)
        {
            var config = new ConfigHelper (arguments, IsVerbose);

            var deviceManager = new DeviceManager ();

            var isVerboseString = config.GetValue ("IsVerbose");
            if (!String.IsNullOrEmpty (isVerboseString))
                Boolean.TryParse (isVerboseString, out IsVerbose);
            deviceManager.IsVerbose = IsVerbose;

            //deviceManager.SleepTimeInSeconds = config.GetInt32 ("SleepTime", 3);
            deviceManager.SleepTimeInSeconds = 3;
            deviceManager.DeviceAddedCommand = config.GetValue ("DeviceAddedCommand");
            deviceManager.DeviceRemovedCommand = config.GetValue ("DeviceRemovedCommand");


            deviceManager.Run ();


        }
    }
}
