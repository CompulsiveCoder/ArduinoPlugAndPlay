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
        public bool AddCommandCompleted = false;
        public bool RemoveCommandCompleted = false;

        public DeviceInfo ()
        {
        }

        public bool DoesMatch (DeviceInfo info)
        {
            if (info == null)
                return false;

            return FamilyName == info.FamilyName &&
            GroupName == info.GroupName &&
            ProjectName == info.ProjectName &&
            BoardType == info.BoardType &&
            ScriptCode == info.ScriptCode;
        }
    }
}

