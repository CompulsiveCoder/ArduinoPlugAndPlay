﻿using System;

namespace ArduinoPlugAndPlay
{
    public class DeviceInfoExtractor
    {
        public string FamilyNamePreText = "Family:";
        public string GroupNamePreText = "Group:";
        public string ProjectNamePreText = "Project:";
        public string BoardTypePreText = "Board:";
        public string DeviceNamePreText = "Device name:";
        public string ScriptCodePreText = "ScriptCode:";
        public string EndDeviceInfoText = "-- End Device Info";

        public DeviceInfoExtractor ()
        {
        }

        public DeviceInfo ExtractInfo (string portName, string output)
        {
            var info = new DeviceInfo ();

            info.Port = portName;

            var lines = output.Split ('\n');

            foreach (var line in lines) {
                if (line.StartsWith (FamilyNamePreText))
                    info.FamilyName = ExtractFamilyName (line);
                else if (line.StartsWith (GroupNamePreText))
                    info.GroupName = ExtractGroupName (line);
                else if (line.StartsWith (ProjectNamePreText))
                    info.ProjectName = ExtractProjectName (line);
                else if (line.StartsWith (DeviceNamePreText))
                    info.DeviceName = ExtractDeviceName (line);
                else if (line.StartsWith (BoardTypePreText))
                    info.BoardType = ExtractBoardType (line);
                else if (line.StartsWith (ScriptCodePreText))
                    info.ScriptCode = ExtractScriptCode (line);
            }

            /*Console.WriteLine ("Extracting device info...");
            Console.WriteLine ("  Family: " + info.FamilyName);
            Console.WriteLine ("  Group: " + info.GroupName);
            Console.WriteLine ("  Project: " + info.ProjectName);
            Console.WriteLine ("  Board: " + info.BoardType);
            Console.WriteLine ("  ScriptCode: " + info.ScriptCode);
            Console.WriteLine ("  Port: " + info.Port);*/

            return info;
        }

        public string ExtractFamilyName (string line)
        {
            return ExtractValueFromLine (line, FamilyNamePreText);
        }

        public string ExtractGroupName (string line)
        {
            return ExtractValueFromLine (line, GroupNamePreText);
        }

        public string ExtractProjectName (string line)
        {
            return ExtractValueFromLine (line, ProjectNamePreText);
        }

        public string ExtractDeviceName (string line)
        {
            return ExtractValueFromLine (line, DeviceNamePreText);
        }

        public string ExtractBoardType (string line)
        {
            return ExtractValueFromLine (line, BoardTypePreText);
        }

        public string ExtractScriptCode (string line)
        {
            return ExtractValueFromLine (line, ScriptCodePreText);
        }

        public string ExtractValueFromLine (string line, string startText)
        {
            var startPosition = line.IndexOf (":") + 1;

            var length = line.Length - startPosition;

            var value = line.Substring (startPosition, length);

            return value.Trim ();
        }
    }
}

