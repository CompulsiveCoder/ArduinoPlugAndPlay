using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Integration
{
	[TestFixture(Category = "Integration")]
	public class SerialToMqttTestFixture : BaseTestFixture
	{
		[Test]
		public void Test_ReadSerialPublishToMqtt()
		{
			using (var helper = new SerialToMqttTestHelper())
			{
				helper.DeviceName = "Device1";

				helper.DevicePort = GetDevicePort();
				helper.DeviceBaudRate = GetDeviceSerialBaudRate();

				helper.TestSerialToMqtt();
			}
		}

	}
}

