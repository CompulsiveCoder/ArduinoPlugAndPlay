using System;
using System.Diagnostics;

namespace ArduinoPlugAndPlay
{
    public class ProcessWrapper
    {
        public string Key = "";

        public Process Process = null;

        public int TryCount = 0;

        public ProcessWrapper (string key, Process process)
        {
            Key = key;
            Process = process;
        }

        public void IncrementTryCount ()
        {
            TryCount++;
        }
    }
}

