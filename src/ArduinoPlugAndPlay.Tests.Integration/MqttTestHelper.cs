using System;
using NUnit.Framework;
using System.IO;
using System.Net.NetworkInformation;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using System.Threading;
using System.Collections.Generic;

namespace ArduinoPlugAndPlay.Tests.Integration
{
	public class MqttTestHelper
	{
		public string DeviceName;
		public string Host;
		public string UserId;
		public string Password;
		public int Port;

		public List<Dictionary<string, string>> Data = new List<Dictionary<string, string>>();
		public Dictionary<string, string> DataEntry = new Dictionary<string, string>();

		public MqttClient Client;

		public string ExistingStatusMessage;

		public int TimeoutWaitingForMqttData = 20 * 1000;

		public TimeoutHelper Timeout = new TimeoutHelper();

		public MqttTestHelper(string deviceName)
		{
			DeviceName = deviceName;
		}

		public MqttTestHelper()
		{
		}

		public void Start(string deviceName)
		{
			Console.WriteLine("");
			Console.WriteLine("Starting MQTT test");
			Console.WriteLine("");

			DeviceName = deviceName;
			// TODO: Remove if not needed. The device name should be set within the test not outside it so this shouldn't be needed.
			//DeviceName = GetSecurityValue("mqtt-device-name", "MONITOR_ESP_DEVICE_NAME");
			Host = GetSecurityValue("mqtt-host", "MQTT_HOST");
			UserId = GetSecurityValue("mqtt-username", "MQTT_USERNAME");
			Password = GetSecurityValue("mqtt-password", "MQTT_PASSWORD");
			Port = Convert.ToInt32(GetSecurityValue("mqtt-port", "MQTT_PORT"));

			Assert.IsNotNullOrEmpty(DeviceName, "DEVICE_NAME environment variable is not set.");
			Assert.IsNotNullOrEmpty(Host, "MQTT_HOST environment variable is not set.");
			Assert.IsNotNullOrEmpty(UserId, "MQTT_USERNAME environment variable is not set.");
			Assert.IsNotNullOrEmpty(Password, "MQTT_PASSWORD environment variable is not set.");
			Assert.Greater(Port, 0, "MQTT_PORT environment variable is not set.");

			Console.WriteLine("Device name: " + DeviceName);
			Console.WriteLine("Host: " + Host);
			Console.WriteLine("Username: " + UserId);
			Console.WriteLine("Port: " + Port);

			Client = new MqttClient(Host, Port, false, null, null, MqttSslProtocols.None);


			var clientId = Guid.NewGuid().ToString();

			Client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;
			Client.Connect(clientId, UserId, Password);

			Client.Subscribe(new string[] { "/" + DeviceName + "/#" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });
		}

		public string GetSecurityValue(string key, string environmentVariable)
		{
			Console.WriteLine("Retrieving security value: " + key);

			var value = Environment.GetEnvironmentVariable(environmentVariable);

			if (String.IsNullOrEmpty(value))
			{
				var projectDirectory = Path.GetFullPath("../..");

				value = File.ReadAllText(Path.Combine(projectDirectory, key + ".security")).Trim();
			}

			return value;
		}

		public void End()
		{
			PublishSuccess();
			Thread.Sleep(2000);
			Client.Disconnect();

			Console.WriteLine("");
			Console.WriteLine("End of MQTT test");
			Console.WriteLine("");
		}

		public void WaitForAccess()
		{
			Console.WriteLine("Waiting for access (ie. ensuring another test isn't already running)");

			var hasAccess = false;

			var maxWaitTime = new TimeSpan(
				0,
				0, // minutes
				10);
			var startWaitTime = DateTime.Now;

			while (!hasAccess)
			{
				WaitForData(1);

				var currentStatus = ExistingStatusMessage;
				var testIsReady = (currentStatus != "Testing");
				Console.WriteLine("Test is ready: " + testIsReady);

				var waitedLongEnough = DateTime.Now.Subtract(startWaitTime) > maxWaitTime;
				Console.WriteLine("Waited long enough: " + waitedLongEnough);

				if (testIsReady || waitedLongEnough)
				{
					Console.WriteLine("Access gained");
					hasAccess = true;
					break;
				}

				Console.Write(".");
				Thread.Sleep(10);
			}
		}

		public void WaitForData(int numberOfEntries)
		{
			Console.WriteLine("Waiting for data...");
			ResetData();
			Timeout.Start();
			while (Data.Count < numberOfEntries)
			{
				Timeout.Check(TimeoutWaitingForMqttData, "Timed out waiting for MQTT data.");
			}
		}

		public double WaitUntilData(int numberOfEntries)
		{
			Console.WriteLine("Waiting for data...");
			ResetData();
			var startTime = DateTime.Now;
			Timeout.Start();
			while (Data.Count < numberOfEntries)
			{
				Timeout.Check(TimeoutWaitingForMqttData, "Timed out waiting for MQTT data.");
			}
			var totalTimeInSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
			return totalTimeInSeconds;
		}

		public void CheckDataEntryTimes(int expectedInterval)
		{
			Assert.IsTrue(Data.Count >= 2, "More data entries are needed");

			var secondLastTime = DateTime.Parse(Data[Data.Count - 2]["Time"]);
			var lastTime = DateTime.Parse(Data[Data.Count - 1]["Time"]);

			Console.WriteLine(secondLastTime.ToString());
			Console.WriteLine(lastTime.ToString());

			var timeSpan = lastTime.Subtract(secondLastTime);

			Console.WriteLine("Time difference (seconds): " + timeSpan.TotalSeconds);

			Assert.AreEqual(expectedInterval, timeSpan.TotalSeconds, "Invalid time difference");
		}

		public void SendCommand(string key, int value)
		{
			SendCommand(key, value.ToString());
		}

		public void SendCommand(string key, string value)
		{
			Console.WriteLine("");
			Console.WriteLine("Sending command...");
			Console.WriteLine("Key: " + key);
			Console.WriteLine("Value: " + value);
			var inTopic = "/" + DeviceName + "/" + key + "/in";

			Console.WriteLine("Topic: " + inTopic);
			Client.Publish(inTopic, Encoding.UTF8.GetBytes(value.ToString()));
			Console.WriteLine("");
		}

		public void PublishSuccess()
		{
			Console.WriteLine("Publishing success");
			ClearErrorMessage();
			PublishStatus(0, "Passed");
		}

		public void PublishError(string error)
		{
			Console.WriteLine("Publishing error: " + error);
			var errorTopic = "/" + DeviceName + "/Error";
			Client.Publish(errorTopic, Encoding.UTF8.GetBytes(error),
				MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
				true);
			PublishStatus(1, "Failed");
		}

		public void ClearErrorMessage()
		{
			var errorTopic = "/" + DeviceName + "/Error";
			Client.Publish(errorTopic, Encoding.UTF8.GetBytes(""));
		}

		public void PublishStatus(int status, string message)
		{
			PublishStatus(status);
			PublishStatusMessage(message);
		}

		public void PublishStatus(int status)
		{
			var statusTopic = "/" + DeviceName + "/Status";
			Client.Publish(
				statusTopic,
				Encoding.UTF8.GetBytes(status.ToString()),
				MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE, // QoS level
				true);
		}

		public void PublishStatusMessage(string message)
		{
			Console.WriteLine("Publishing status message: " + message);
			var statusMessageTopic = "/" + DeviceName + "/StatusMessage";
			Client.Publish(statusMessageTopic, Encoding.UTF8.GetBytes(message),
				MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, // QoS level
				true);
		}

		public void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
		{
			var topic = e.Topic;

			var value = System.Text.Encoding.Default.GetString(e.Message);

			var key = GetTopicKey(topic);

			DataEntry[key] = value;

			if (key == "StatusMessage")
				ExistingStatusMessage = value;

			if (key == "Time" && !IsDuplicateEntry(DataEntry))
			{
				Data.Add(DataEntry);
				DataEntry = new Dictionary<string, string>();
			}
		}

		public bool IsDuplicateEntry(Dictionary<string, string> dataEntry)
		{
			foreach (var entry in Data)
			{
				if (entry["Time"] == dataEntry["Time"])
					return true;
			}

			return false;
		}

		public void PrintDataEntry(Dictionary<string, string> dataEntry)
		{
			if (dataEntry != null)
			{
				Console.WriteLine("");
				Console.WriteLine("----- MQTT Data Start");
				foreach (var key in dataEntry.Keys)
				{
					Console.Write(key + ":" + dataEntry[key] + ";");
				}
				Console.WriteLine(";");
				Console.WriteLine("----- MQTT Data End");
				Console.WriteLine("");
			}
		}

		public string GetTopicKey(string topic)
		{
			var parts = topic.Split('/');
			var key = parts[2];

			return key;
		}

		public void ResetData()
		{
			Data = new List<Dictionary<string, string>>();
		}
	}
}