using System;
using duinocom;
using System.Threading;

namespace ArduinoPlugAndPlay
{
    public class DeviceReaderWriter
    {
        public SerialClient Client;

        public int TimeoutReadingLineInSeconds = 15;
        public TimeoutHelper Timeout = new TimeoutHelper ();

        public DeviceReaderWriter ()
        {
        }

        public virtual void Open (string portName, int baudRate)
        {
            Console.WriteLine ("Opening serial port: " + portName);

            Client = new SerialClient (portName, baudRate);
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

