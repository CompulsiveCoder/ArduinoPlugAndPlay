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
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
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
            Console.WriteLine ("Starting Arduino Plug and Play");
            Console.WriteLine ("Checking device list...");
           
            var config = new ConfigHelper (arguments, IsVerbose);

            var deviceManager = new DeviceManager ();

            deviceManager.DeviceAddedCommand = config.GetValue ("DeviceAddedCommand");
            deviceManager.DeviceRemovedCommand = config.GetValue ("DeviceRemovedCommand");

            deviceManager.Run ();


        }
    }
}