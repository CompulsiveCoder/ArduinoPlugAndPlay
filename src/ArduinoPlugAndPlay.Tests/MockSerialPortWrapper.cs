using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
	public class MockSerialPortWrapper : SerialPortWrapper
	{
		public string[] MockPortNames = new string[]{};

		public MockSerialPortWrapper ()
		{
		}

		public MockSerialPortWrapper(params string[] mockPortNames)
		{
			MockPortNames = mockPortNames;
		}

		public override string[] GetPortNames ()
		{
			// TODO: Remove if not needed
			//return base.GetPortNames ();
			return MockPortNames;
		}

		public void ConnectDevice(string portName)
		{
			var list = new List<string> (MockPortNames);
			if (!list.Contains (portName))
				list.Add (portName);
			MockPortNames = list.ToArray ();
		}

		public void DisconnectDevice(string portName)
		{
			var list = new List<string> (MockPortNames);
			if (list.Contains (portName))
				list.Remove (portName);
			MockPortNames = list.ToArray ();
		}
	}
}

