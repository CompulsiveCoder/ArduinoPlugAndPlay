using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockPlatformioWrapper : PlatformioWrapper
    {
        public List<string> MockDevices = new List<string> ();

        public MockPlatformioWrapper ()
        {
        }

        public void ConnectDevice (string portName)
        {
            Console.WriteLine ("Virtually (mock) connecting device: " + portName);

            if (!MockDevices.Contains (portName))
                MockDevices.Add (portName);
        }

        public void DisconnectDevice (string portName)
        {
            Console.WriteLine ("Virtually (mock) disconnecting device: " + portName);

            if (MockDevices.Contains (portName))
                MockDevices.Remove (portName);
        }

        public override string[] GetDeviceList ()
        {
            return MockDevices.ToArray ();
        }

        public override bool AreDevicesDetected ()
        {
            return MockDevices.Count > 0;
        }
    }
}

