using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay
{
    public class ProcessWrapper
    {
        public string Key = "";
        public string Action = "";

        public Process Process = null;

        public int TryCount = 0;

        public DeviceInfo Info { get; set; }

        public ProcessWrapper (string action, DeviceInfo info, Process process)
        {
            Key = action + "-" + info.Port;
            Action = action;
            Info = info;
            Process = process;
        }

        public void IncrementTryCount ()
        {
            TryCount++;
        }
    }
}

