using System;
using duinocom;
using System.Threading;

namespace ArduinoPlugAndPlay
{
    public class SerialDeviceReaderWriter
    {
        public SerialClient Client;

        public int TimeoutReadingLineInSeconds = 15;
        public TimeoutHelper Timeout = new TimeoutHelper ();

        public SerialDeviceReaderWriter ()
        {
        }

        public virtual void Open (string portName)
        {
            Open (portName, 9600);
        }

        public virtual void Open (string portName, int baudRate)
        {
            Console.WriteLine ("Opening serial port: " + portName);

            var fullPortName = portName;
            if (!portName.Contains ("/dev/"))
                fullPortName = "/dev/" + portName;

            Console.WriteLine ("  Full port name: " + fullPortName);

            Client = new SerialClient (fullPortName, baudRate);
            Client.Open ();

            Console.WriteLine ("  Serial port is open");
        }

        public virtual void Close ()
        {
            Console.WriteLine ("Closing serial port: " + Client.Port.PortName);

            Client.Close ();            

            Console.WriteLine ("  Serial port is closed");
        }

        public virtual string Read ()
        {
            return Client.Read ();
        }

        public virtual string ReadLine ()
        {
            Timeout.Start ();
            while (Client.Port.BytesToRead == 0) {
                Thread.Sleep (10);
                Timeout.Check (TimeoutReadingLineInSeconds * 1000, "Timed out reading a line from the serial port.");
            }
            return Client.ReadLine ();
        }

        public virtual void WriteLine (string lineOfText)
        {
            Client.WriteLine (lineOfText);
        }
    }
}

