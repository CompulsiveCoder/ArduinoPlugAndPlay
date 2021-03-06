﻿using System;
using System.IO;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay
{
    public class DeviceInfoFileManager
    {
        public string InfoDirectory { get; set; }

        public DeviceInfoFileManager ()
        {            
            InfoDirectory = Path.Combine (Environment.CurrentDirectory, "devices");
        }

        public DeviceInfoFileManager (string workingDirectory)
        {
            InfoDirectory = workingDirectory;

            if (!InfoDirectory.StartsWith ("/"))
                InfoDirectory = Path.GetFullPath (workingDirectory);

            if (!Directory.Exists (InfoDirectory))
                Directory.CreateDirectory (InfoDirectory);
        }

        public void WriteInfoToFile (DeviceInfo info)
        {
            var deviceInfoDir = Path.Combine (InfoDirectory, info.Port.Replace ("/dev/", ""));

            if (!Directory.Exists (deviceInfoDir))
                Directory.CreateDirectory (deviceInfoDir);

            File.WriteAllText (Path.Combine (deviceInfoDir, "family.txt"), info.FamilyName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "group.txt"), info.GroupName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "project.txt"), info.ProjectName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "device-name.txt"), info.DeviceName);
            File.WriteAllText (Path.Combine (deviceInfoDir, "board.txt"), info.BoardType);
            File.WriteAllText (Path.Combine (deviceInfoDir, "script-code.txt"), info.ScriptCode);
            File.WriteAllText (Path.Combine (deviceInfoDir, "port.txt"), info.Port);
            File.WriteAllText (Path.Combine (deviceInfoDir, "add-command-completed.txt"), info.AddCommandCompleted.ToString ());
            File.WriteAllText (Path.Combine (deviceInfoDir, "remove-command-completed.txt"), info.RemoveCommandCompleted.ToString ());
        }

        public DeviceInfo ReadInfoFromFile (string devicePort)
        {
            //Console.WriteLine ("Reading device info from file...");

            var deviceFolder = Path.Combine (InfoDirectory, devicePort.Replace ("/dev/", ""));

            var info = new DeviceInfo ();
            info.Port = devicePort;

            if (Directory.Exists (deviceFolder)) {
                //Console.WriteLine ("Device info directory:");
                //Console.WriteLine (deviceFolder);

                if (File.Exists (Path.Combine (deviceFolder, "family.txt")))
                    info.FamilyName = File.ReadAllText (Path.Combine (deviceFolder, "family.txt")).Trim ();
                if (File.Exists (Path.Combine (deviceFolder, "group.txt")))
                    info.GroupName = File.ReadAllText (Path.Combine (deviceFolder, "group.txt")).Trim ();
                if (File.Exists (Path.Combine (deviceFolder, "project.txt")))
                    info.ProjectName = File.ReadAllText (Path.Combine (deviceFolder, "project.txt")).Trim ();
                if (File.Exists (Path.Combine (deviceFolder, "device-name.txt")))
                    info.DeviceName = File.ReadAllText (Path.Combine (deviceFolder, "device-name.txt")).Trim ();
                if (File.Exists (Path.Combine (deviceFolder, "board.txt")))
                    info.BoardType = File.ReadAllText (Path.Combine (deviceFolder, "board.txt")).Trim ();
                if (File.Exists (Path.Combine (deviceFolder, "script-code.txt")))
                    info.ScriptCode = File.ReadAllText (Path.Combine (deviceFolder, "script-code.txt")).Trim ();

                if (File.Exists (Path.Combine (deviceFolder, "add-command-completed.txt")))
                    bool.TryParse (File.ReadAllText (Path.Combine (deviceFolder, "add-command-completed.txt")).Trim (), out info.AddCommandCompleted);
                if (File.Exists (Path.Combine (deviceFolder, "remove-command-completed.txt")))
                    bool.TryParse (File.ReadAllText (Path.Combine (deviceFolder, "remove-command-completed.txt")).Trim (), out info.RemoveCommandCompleted);
            }

            return info;
        }

        public DeviceInfo[] ReadAllDevicesFromFile ()
        {
            //Console.WriteLine ("Reading all devices from file...");
            //Console.WriteLine ("Current directory:");
            //Console.WriteLine (WorkingDirectory);

            var list = new List<DeviceInfo> ();

            if (Directory.Exists (InfoDirectory)) {
                foreach (string dir in Directory.GetDirectories(InfoDirectory)) {
                    var port = Path.GetFileName (dir);

                    var info = ReadInfoFromFile (port);

                    list.Add (info);
                }
            }

            return list.ToArray ();
        }

        public bool DeviceExists (string devicePort)
        {
            var deviceFolder = Path.Combine (InfoDirectory, devicePort.Replace ("/dev/", ""));

            return Directory.Exists (deviceFolder);
        }

        public void DeleteInfoFromFile (string devicePort)
        {
            if (!String.IsNullOrEmpty (devicePort)) {
                //Console.WriteLine ("Deleting device info from file...");

                var deviceInfoDir = Path.Combine (InfoDirectory, devicePort.Replace ("/dev/", ""));
                //Console.WriteLine ("Deleting device info:");
                //Console.WriteLine (deviceInfoDir);

                if (Directory.Exists (deviceInfoDir))
                    Directory.Delete (deviceInfoDir, true);
            }
        }
    }
}

