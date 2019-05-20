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

        public bool HasStarted = false;

        public bool HasExited {
            get {
                if (HasStarted) {
                    if (Process.HasExited)
                        return true;
                }
                return false;
            }
        }

        public DateTime StartTime = DateTime.MinValue;

        public TimeSpan Duration {
            get {
                return DateTime.Now.Subtract (StartTime);
            }
        }

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

        public void Start ()
        {
            StartTime = DateTime.Now;
            HasStarted = true;
            Process.Start ();
        }
    }
}

