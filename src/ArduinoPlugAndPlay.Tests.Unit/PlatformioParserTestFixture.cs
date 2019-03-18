using System;
using NUnit.Framework;
using System.Text;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests.Unit
{
    [TestFixture (Category = "Unit")]
    public class PlatformioParserTestFixture : BaseTestFixture
    {
        public PlatformioParserTestFixture ()
        {
        }

        [Test]
        public void Test_AreDevicesDetected_false ()
        {
            var starter = new ProcessStarter ();

            var parser = new PlatformioParser (starter);

            starter.OutputBuilder = new StringBuilder ();
            starter.OutputBuilder.Append (MockOutputs.DeviceListZeroDevicesOutput);

            var expectedResult = false;

            var actualResult = parser.AreDevicesDetected ();

            Assert.AreEqual (expectedResult, actualResult, "Result doesn't match the expected result.");
        }

        [Test]
        public void Test_AreDevicesDetected_true ()
        {
            var starter = new ProcessStarter ();

            var parser = new PlatformioParser (starter);

            starter.OutputBuilder = new StringBuilder ();
            starter.OutputBuilder.Append (MockOutputs.GetDeviceListOutput (2));

            var expectedResult = true;

            var actualResult = parser.AreDevicesDetected ();

            Assert.AreEqual (expectedResult, actualResult, "Result doesn't match the expected result.");
        }


        [Test]
        public void Test_GetDeviceList_0Devices ()
        {
            var starter = new ProcessStarter ();

            var parser = new PlatformioParser (starter);

            starter.OutputBuilder = new StringBuilder ();
            starter.OutputBuilder.Append (MockOutputs.DeviceListZeroDevicesOutput);

            var expectedResult = new string[]{ };

            var actualResult = parser.ParseDeviceList ();

            Assert.AreEqual (expectedResult, actualResult, "Result doesn't match the expected result.");
        }

        [Test]
        public void Test_GetDeviceList_5Devices ()
        {
            var starter = new ProcessStarter ();

            var parser = new PlatformioParser (starter);

            starter.OutputBuilder = new StringBuilder ();
            starter.OutputBuilder.Append (MockOutputs.GetDeviceListOutput (5));

            var resultList = new List<string> ();
            for (int i = 0; i < 5; i++) {
                resultList.Add ("/dev/ttyUSB" + i);
            }
            var expectedResult = resultList.ToArray ();

            var actualResult = parser.ParseDeviceList ();

            Assert.AreEqual (expectedResult, actualResult, "Result doesn't match the expected result.");
        }
    }
}

