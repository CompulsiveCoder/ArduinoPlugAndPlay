using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay
{
    public class ProcessWrapper
    {
        public string Key = "";

        public string Action = "";
        public string Port = "";

        public Process Process = null;

        public int TryCount = 0;

        public ProcessWrapper (string action, string port, Process process)
        {
            Key = action + "-" + port;
            Action = action;
            Port = port;
            Process = process;
        }

        public void IncrementTryCount ()
        {
            TryCount++;
        }
    }
}

