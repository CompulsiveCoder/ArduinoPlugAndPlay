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
            Console.WriteLine ("Arguments:");
            foreach (var arg in args)
                Console.WriteLine (arg);

            var arguments = new Arguments (args);

            Run (arguments);
        }

        public static void Run (Arguments arguments)
        {
            var config = new ConfigHelper (arguments, IsVerbose);

            var deviceManager = new DeviceManager ();

            deviceManager.SleepTimeInSeconds = Convert.ToInt32 (config.GetValue ("SleepTime"));
            deviceManager.DeviceAddedCommand = config.GetValue ("DeviceAddedCommand");
            deviceManager.DeviceRemovedCommand = config.GetValue ("DeviceRemovedCommand");

            deviceManager.Run ();


        }
    }
}