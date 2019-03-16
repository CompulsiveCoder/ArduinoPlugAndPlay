using System;

namespace ArduinoPlugAndPlay
{
    public class DeviceInfo
    {
        public string FamilyName = "";
        public string GroupName = "";
        public string ProjectName = "";
        public string BoardType = "";
        public string Port = "";
        public bool AddCommandCompleted = false;
        public bool RemoveCommandCompleted = false;

        public DeviceInfo ()
        {
        }
    }
}

