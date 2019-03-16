using System;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay
{
    public class PlatformioParser
    {
        public ProcessStarter Starter = new ProcessStarter ();

        public PlatformioParser (ProcessStarter starter)
        {
            Starter = starter;
        }

        public string[] ParseDeviceList ()
        {
            var list = new List<string> ();

            var lines = Starter.Output.Split ('\n');

            if (lines.Length > 0) {
                // Loop backwards through the lines so the device numbers show up in the correct order
                for (int i = lines.Length - 1; i >= 0; i--) {
                    var line = lines [i];
                    if (line.Trim ().StartsWith ("/dev/")) {
                        list.Add (line.Trim ());
                    }
                }
            }

            return list.ToArray ();
        }

        public bool AreDevicesDetected ()
        {
            var output = Starter.Output;

            var deviceListIsEmpty = String.IsNullOrWhiteSpace (output);

            return !deviceListIsEmpty;
        }

    }
}

