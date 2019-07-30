using System;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture (Category = "Unit")]
    public class DeviceInfoExtractorTestFixture : BaseTestFixture
    {
        [Test]
        public void Test_ExtractDeviceInfo ()
        {
            var exampleInfo = GetExampleDeviceInfo ();

            var exampleOutput = MockOutputs.GetDeviceSerialOutput (exampleInfo);
            
            var extractor = new DeviceInfoExtractor ();

            var info = extractor.ExtractInfo (exampleInfo.Port, exampleOutput);

            Assert.AreEqual (exampleInfo.FamilyName, info.FamilyName, "Family names don't match");
            Assert.AreEqual (exampleInfo.GroupName, info.GroupName, "Group names don't match");
            Assert.AreEqual (exampleInfo.ProjectName, info.ProjectName, "Project names don't match");
            Assert.AreEqual (exampleInfo.BoardType, info.BoardType, "Board types don't match");
            Assert.AreEqual (exampleInfo.ScriptCode, info.ScriptCode, "Script codes don't match");
            Assert.AreEqual (exampleInfo.Port, info.Port, "Ports don't match");
        }
    }
}

