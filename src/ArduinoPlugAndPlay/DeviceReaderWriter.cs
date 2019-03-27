using System;
using duinocom;

namespace ArduinoPlugAndPlay
{
    public class DeviceReaderWriter
    {
        public SerialClient Client;

        public DeviceReaderWriter ()
        {
        }

        public virtual void Open (string portName, int baudRate)
        {
            Client = new SerialClient (portName, baudRate);
            Client.Open ();
        }

        public virtual void Close ()
        {
            Client.Close ();            
        }

        public virtual string Read ()
        {
            return Client.Read ();
        }

        public virtual string ReadLine ()
        {
            return Client.ReadLine ();
        }

        public virtual void WriteLine (string lineOfText)
        {
            Client.WriteLine (lineOfText);
        }
    }
}

