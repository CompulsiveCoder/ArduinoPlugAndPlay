using System;
using NUnit.Framework;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockPlatformioWrapper : PlatformioWrapper
    {
        public List<string> MockDevices = new List<string> ();

        public MockDeviceOutputs MockOutputs = new MockDeviceOutputs ();

        public MockPlatformioWrapper ()
        {
        }

        public void ConnectDevice (string portName)
        {
            Console.WriteLine ("Virtually (mock) connecting device: " + portName);

            if (!MockDevices.Contains (portName.Replace ("/dev/", "")))
                MockDevices.Add (portName.Replace ("/dev/", ""));
        }

        public void DisconnectDevice (string portName)
        {
            Console.WriteLine ("Virtually (mock) disconnecting device: " + portName);

            if (MockDevices.Contains (portName.Replace ("/dev/", "")))
                MockDevices.Remove (portName.Replace ("/dev/", ""));
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

