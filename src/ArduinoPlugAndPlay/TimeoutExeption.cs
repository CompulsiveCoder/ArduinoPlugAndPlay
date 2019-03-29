using System;

namespace ArduinoPlugAndPlay
{
    public class TimeoutExeption : Exception
    {
        public TimeoutExeption (string message) : base (message)
        {
        }
    }
}

