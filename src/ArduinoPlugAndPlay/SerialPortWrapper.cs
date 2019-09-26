using System;
using System.IO.Ports;

namespace ArduinoPlugAndPlay
{
	public class SerialPortWrapper
	{
		public SerialPortWrapper ()
		{
		}

		public virtual string[] GetPortNames()
		{
			return SerialPort.GetPortNames ();
		}
	}
}

