using System;

namespace ArduinoPlugAndPlay
{
    public class DeviceInfo
    {
        public string FamilyName = "";
        public string GroupName = "";
        public string ProjectName = "";
        public string BoardType = "";
        public string ScriptCode = "";
        public string Port = "";
        public string DeviceName = "";
        public bool AddCommandCompleted = false;
        public bool RemoveCommandCompleted = false;

        public DeviceInfo ()
        {
        }

        public bool DoesMatch (DeviceInfo info)
        {
            var doesMatch = true;

            Console.WriteLine ("  Checking whether device info matches...");
            if (info == null) {
                Console.WriteLine ("    Info is null. Doesn't match.");
                return false;
            }

            if (!DoesMatch ("DeviceName", DeviceName, info.DeviceName)) {
                doesMatch = false;
            }
            if (!DoesMatch ("FamilyName", FamilyName, info.FamilyName)) {
                doesMatch = false;
            }
            if (!DoesMatch ("GroupName", GroupName, info.GroupName)) {
                doesMatch = false;
            }
            if (!DoesMatch ("ProjectName", ProjectName, info.ProjectName)) {
                doesMatch = false;
            }
            if (!DoesMatch ("BoardType", BoardType, info.BoardType)) {
                doesMatch = false;
            }
            if (!DoesMatch ("Port", Port, info.Port)) {
                doesMatch = false;
            }
            if (!DoesMatch ("ScriptCode", ScriptCode, info.ScriptCode)) {
                doesMatch = false;
            }

            return doesMatch;
        }

        public bool DoesMatch (string label, string value1, string value2)
        {
            var doesMatch = value1.Trim () == value2.Trim ();
            var operatorType = (doesMatch ? "==" : "!=");
            Console.WriteLine ("    " + label);
            Console.WriteLine ("      '" + value1 + "' " + operatorType + " '" + value2 + "'");

            return doesMatch;
        }
    }
}

