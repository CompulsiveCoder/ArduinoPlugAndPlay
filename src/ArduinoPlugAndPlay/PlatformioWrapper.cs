using System;
using System.Threading;

namespace ArduinoPlugAndPlay
{
    public class PlatformioWrapper
    {
        public ProcessStarter Starter = new ProcessStarter ();

        public PlatformioParser Parser;

        public DeviceInfoExtractor Extractor = new DeviceInfoExtractor ();

        public PlatformioWrapper ()
        {
            Parser = new PlatformioParser (Starter);
        }

        public virtual string[] GetDeviceList ()
        {
            var deviceList = new string[]{ };

            Starter.Start ("pio device list");

            var devicesDetected = Parser.AreDevicesDetected ();

            if (devicesDetected)
                deviceList = Parser.ParseDeviceList ();

            Starter.ClearOutput ();

            return deviceList;
        }

        public virtual bool AreDevicesDetected ()
        {
            Starter.Start ("pio device list");

            var devicesDetected = Parser.AreDevicesDetected ();

            Starter.ClearOutput ();

            return devicesDetected;
        }
    }
}

