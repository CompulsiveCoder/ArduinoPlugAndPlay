using System;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockDeviceReaderWriter : DeviceReaderWriter
    {
        public string MockOutput = "";

        public string RemainingMockOutput = "";

        public bool MockOutputHasBeenSet = false;

        public MockDeviceReaderWriter ()
        {
        }

        public void SetMockOutput (string output)
        {
            MockOutput = output;
            RemainingMockOutput = output;
            MockOutputHasBeenSet = true;
        }

        public override void Open (string portName, int baudRate)
        {
            Console.WriteLine ("Opening virtual (mock) connection to device: " + portName);
        }

        public override void Close ()
        {
            Console.WriteLine ("Closing virtual (mock) connection to device");
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

            if (String.IsNullOrEmpty (RemainingMockOutput))
                return String.Empty;
            else if (!RemainingMockOutput.Contains ("\n"))
                return RemainingMockOutput;
            else {
                var line = RemainingMockOutput.Substring (0, RemainingMockOutput.IndexOf ('\n'));

                RemainingMockOutput = RemainingMockOutput.Substring (line.Length + 1, RemainingMockOutput.Length - line.Length - 1);

                Console.WriteLine (line);

                return line;
            }
        }
    }
}

