using System;
using System.Threading;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockSerialDeviceReaderWriter : SerialDeviceReaderWriter
    {
        public bool MockOutputHasBeenSet = false;

        public bool EnableVirtualDelay = false;

        public Dictionary<string, string> MockSerialDeviceOutput = new Dictionary<string, string> ();

        public string CurrentOpenPort = "";

        public MockSerialDeviceReaderWriter ()
        {
        }

        public void SetMockOutput (string portName, string output)
        {
            MockSerialDeviceOutput.Add (portName.Replace ("/dev/", ""), output);
            MockOutputHasBeenSet = true;
        }

        public override void Open (string portName)
        {
            Open (portName.Replace ("/dev/", ""), 9600);
        }

        public override void Open (string portName, int baudRate)
        {
            Console.WriteLine ("Opening virtual (mock) connection to device: " + portName);
            CurrentOpenPort = portName;
        }

        public override void Close ()
        {
            Console.WriteLine ("Closing virtual (mock) connection to device: " + CurrentOpenPort);
            CurrentOpenPort = "";
        }

        public override string Read ()
        {
            throw new NotImplementedException ();
        }

        public override string ReadLine ()
        {
            //Console.WriteLine ("Reading virtual (mock) line from device...");

            if (!MockOutputHasBeenSet)
                throw new Exception ("The mock output hasn't been set yet. Use the SetMockOutput function.");

            if (String.IsNullOrEmpty (CurrentOpenPort))
                throw new Exception ("No port has been opened.");

            var output = MockSerialDeviceOutput [CurrentOpenPort];

            if (String.IsNullOrEmpty (output))
                return String.Empty;
            else if (!output.Contains ("\n"))
                return output;
            else {
                var line = output.Substring (0, output.IndexOf ('\n'));

                MockSerialDeviceOutput [CurrentOpenPort] = output.Substring (line.Length + 1, output.Length - line.Length - 1);

                Console.WriteLine (line);

                if (EnableVirtualDelay) {
                    // Sleep for a while because a ReadLine command normally takes time
                    Thread.Sleep (200);
                }

                return line;
            }
        }

        public override void WriteLine (string lineOfText)
        {
            
        }
    }
}

