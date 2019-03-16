using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace ArduinoPlugAndPlay.Tests.Integration
{
	public class SerialToMqttTestHelper : DeviceHardwareTestHelper
	{
		public string DeviceName = "Device1";
		public string SubscriveTopics = "V";
		public string SummaryTopic = "V";

		public ProcessStarter Starter = new ProcessStarter ();

		public MqttTestHelper Mqtt = new MqttTestHelper();

		public SerialToMqttTestHelper()
		{
		}

		public void TestSerialToMqtt()
		{
			WriteTitleText("Starting read from serial and publish to MQTT test");

			ConnectDevices(false);

			// Generate a random number to publish from the device to MQTT
			var randomNumber = new Random ().Next (100);

			// Set the random number on the device
			SendDeviceCommand ("V" + randomNumber);

			var dataEntry = WaitForDataEntry();

			AssertDataValueEquals(dataEntry, "V", randomNumber);

			LaunchMqttBridge (randomNumber);
		}

		public void LaunchMqttBridge(int randomNumber)
		{
			// Start the MQTT helper before trying to access the MQTT credentials
			Mqtt.Start (DeviceName);

			var binDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly ().Location);

			Console.WriteLine ("Bin dir: " + binDir);

			var exeFile = Path.Combine (binDir, "ArduinoPlugAndPlay.exe");

			// Assemble the MQTT bridge command
			var cmd = String.Format("timeout 5s mono {0} --Host={1} --UserId={2} --Password={3} --MqttPort={4} --DeviceName={5} --SerialPort={6} --SubscribeTopics=V --SummaryKey=V",
				exeFile,
				Mqtt.Host, Mqtt.UserId, Mqtt.Password, Mqtt.Port,
				DeviceName, DevicePort);

			// This is disabled because it would output the password to the log which isn't a good idea outside a local workstation
			//Console.WriteLine ("MQTT bridge command:");
			//Console.WriteLine (cmd);

			Console.WriteLine("Random number value: " + randomNumber);

			// Reset the existing MQTT data
			Mqtt.ResetData ();

			// Launch the MQTT bridge utility with a timeout
			Starter.Start (cmd);

			Console.WriteLine (Starter.Output);

			// Ensure no exceptions occurred with the MQTT bridge
			Assert.IsFalse (Starter.Output.Contains ("Exception"), "MQTT bridge raised an exception.");

			var dataCount = Mqtt.Data.Count;

			Assert.Greater (dataCount, 2, "Not enough MQTT data entries detected.");

			// Get the latest MQTT entry
			var latestEntry = Mqtt.Data [Mqtt.Data.Count - 1];

			// Check the values are correct
			Assert.AreEqual (randomNumber, Convert.ToInt32(latestEntry ["V"]), "The specified random number wasn't published.");
		}
	}
}
