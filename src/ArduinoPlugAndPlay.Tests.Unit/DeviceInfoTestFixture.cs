using System;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture (Category = "Unit")]
    public class DeviceInfoTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_DoesMatch_True ()
        {
            var deviceInfo1 = new DeviceInfo ();
            deviceInfo1.FamilyName = "ExampleFamily";
            deviceInfo1.GroupName = "ExampleGroup";
            deviceInfo1.ProjectName = "ExampleProject";
            deviceInfo1.BoardType = "ExampleBoard";
            deviceInfo1.DeviceName = "ExampleDeviceName";
            deviceInfo1.ScriptCode = "ExampleScriptCode";
            deviceInfo1.Port = "ExamplePort";

            var deviceInfo2 = new DeviceInfo ();
            deviceInfo2.FamilyName = "ExampleFamily";
            deviceInfo2.GroupName = "ExampleGroup";
            deviceInfo2.ProjectName = "ExampleProject";
            deviceInfo2.BoardType = "ExampleBoard";
            deviceInfo2.DeviceName = "ExampleDeviceName";
            deviceInfo2.ScriptCode = "ExampleScriptCode";
            deviceInfo2.Port = "ExamplePort";

            var doesMatch = deviceInfo1.DoesMatch (deviceInfo2);

            Assert.IsTrue (doesMatch, "Doesn't match when it should.");
        }

        [Test]
        public void Test_DoesMatch_False ()
        {
            var deviceInfo1 = new DeviceInfo ();
            deviceInfo1.FamilyName = "ExampleFamily";
            deviceInfo1.GroupName = "ExampleGroup";
            deviceInfo1.ProjectName = "ExampleProject";
            deviceInfo1.BoardType = "ExampleBoard";
            deviceInfo1.DeviceName = "ExampleDeviceName";
            deviceInfo1.ScriptCode = "ExampleScriptCode";
            deviceInfo1.Port = "ExamplePort";

            var deviceInfo2 = new DeviceInfo ();
            deviceInfo2.FamilyName = "ExampleFamily";
            deviceInfo2.GroupName = "ExampleGroup";
            deviceInfo2.ProjectName = "ExampleProject";
            deviceInfo2.BoardType = "ExampleBoard";
            deviceInfo2.DeviceName = "DifferentExampleDeviceName";
            deviceInfo2.ScriptCode = "ExampleScriptCode";
            deviceInfo2.Port = "ExamplePort";

            var doesMatch = deviceInfo1.DoesMatch (deviceInfo2);

            Assert.IsFalse (doesMatch, "Matches when it shouldn't.");
        }
    }
}

