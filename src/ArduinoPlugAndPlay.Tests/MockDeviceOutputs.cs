﻿using System;

namespace ArduinoPlugAndPlay.Tests
{
    public class MockDeviceOutputs
    {
        public MockDeviceOutputs ()
        {
        }

        public string GetDeviceSerialOutput (DeviceInfo deviceInfo)
        {
            var output = @"Starting device...
-- Start Device Info
Family: " + deviceInfo.FamilyName + @"
Group: " + deviceInfo.GroupName + @"
Project: " + deviceInfo.ProjectName + @"
Device name: " + deviceInfo.DeviceName + @"
Board: " + deviceInfo.BoardType + @"
ScriptCode: " + deviceInfo.ScriptCode + @"
-- End Device Info
";

            return output;
        }
        // TODO: Remove if not needed. Should be obsolete.
        /*public string GetDeviceListOutput (int numberOfDevices)
        {
            var template = @"/dev/ttyUSB{0}
------------
Hardware ID: USB VID:PID=1A86:7523 LOCATION=3-1.4.3
Description: USB2.0-Serial

";

            var output = String.Empty;

            for (int i = 0; i < numberOfDevices; i++) {
                output = String.Format (template, i) + output;
            }

            return output;
        }*/
        public string DeviceListZeroDevicesOutput {
            get {
                return " \n ";
            }
        }
    }
}

