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

            deviceManager.IsVerbose = config.GetBoolean ("IsVerbose");

            deviceManager.SleepTimeInSeconds = config.GetInt32 ("SleepTime", 1);
            deviceManager.TimeoutReadingDeviceInfoInSeconds = config.GetInt32 ("TimeoutReadingDeviceInfo", 30);
            deviceManager.CommandTimeoutInSeconds = config.GetInt32 ("CommandTimeout", 120);

            deviceManager.USBDeviceConnectedCommand = config.GetValue ("USBDeviceConnectedCommand");
            deviceManager.USBDeviceDisconnectedCommand = config.GetValue ("USBDeviceDisconnectedCommand");

            deviceManager.SmtpServer = config.GetValue ("SmtpServer");
            deviceManager.EmailAddress = config.GetValue ("EmailAddress");
            deviceManager.SmtpUsername = config.GetValue ("SmtpUsername");
            deviceManager.SmtpPassword = config.GetValue ("SmtpPassword");
            deviceManager.SmtpPort = config.GetInt32 ("SmtpPort", 25);

            deviceManager.IgnoredSerialPorts = config.GetArray ("IgnoredSerialPorts");

            deviceManager.Run ();


        }
    }
}
