using System;
using ArduinoSerialControllerClient;
using duinocom;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using System.Threading;

namespace ArduinoPlugAndPlay.Tests.Integration
{
	public class HardwareTestHelper : IDisposable
	{
		public SerialClient DeviceClient = null;
		public bool DeviceIsEnabled = false;

		public ArduinoSerialDevice SimulatorClient = null;
		public bool SimulatorIsEnabled = false;

		public string DevicePort;
		public int DeviceBaudRate = 0;

		public string SimulatorPort;
		public int SimulatorBaudRate = 0;

		public int DelayAfterConnectingToHardware = 2 * 1000;
		public int DelayAfterDisconnectingFromHardware = 1 * 1000;

		public string DataPrefix = "D;";
		public string DataPostFix = ";;";

		public int TimeoutWaitingForResponse = 20 * 1000;

		public int AnalogPinMaxValue = 1023;

		public bool On = true;
		public bool Off = false;

		public string FullDeviceOutput;

		public int ResetTriggerPin = 4;

		public string IrrigatorStartText = "Starting soil moisture monitor";

		public TimeoutHelper Timeout = new TimeoutHelper();

		public HardwareTestHelper()
		{
		}

		#region Console Output Functions
		public void WriteTitleText(string titleText)
		{
			Console.WriteLine("========================================");
			Console.WriteLine(titleText);
			Console.WriteLine("");
		}

		public void WriteSubTitleText(string subTitleText)
		{
			Console.WriteLine("----------------------------------------");
			Console.WriteLine(subTitleText);
			Console.WriteLine("");
		}

		public void WriteParagraphTitleText(string text)
		{
			Console.WriteLine("");
			Console.WriteLine(text);
		}
		#endregion

		#region Enable Device/Simulator Functions
		public void ConnectDevices()
		{
			ConnectDevices(true);
		}

		public virtual void ConnectDevices(bool enableSimulator)
		{
			if (enableSimulator)
				ConnectSimulator();

			ConnectDevice();

			WaitForDevicesToConnect();
		}

		public void ConnectDevice()
		{
			if (String.IsNullOrEmpty(DevicePort))
				throw new Exception("The 'DevicePort' property has not been set.");

			if (DeviceBaudRate == 0)
				throw new Exception("The 'DeviceBaudRate' property has not been set.");

			Console.WriteLine("Enabling target hardware device...");

			DeviceClient = new SerialClient(DevicePort, DeviceBaudRate);

			try
			{
				DeviceClient.Open();
			}
			catch (IOException ex)
			{
				HandleConnectionIOException("target", DevicePort, DeviceBaudRate, ex);
			}

			DeviceIsEnabled = true;

			Console.WriteLine("");
		}

		public void ConnectSimulator()
		{
			if (String.IsNullOrEmpty(SimulatorPort))
				throw new Exception("The 'SimulatorPort' property has not been set.");

			if (SimulatorBaudRate == 0)
				throw new Exception("The 'SimulatorBaudRate' property has not been set.");

			Console.WriteLine("Enabling simulator hardware device...");

			SimulatorClient = new ArduinoSerialDevice(SimulatorPort, SimulatorBaudRate);

			try
			{
				SimulatorClient.Connect();
			}
			catch (IOException ex)
			{
				HandleConnectionIOException("simulator", SimulatorPort, SimulatorBaudRate, ex);
			}

			SimulatorIsEnabled = true;

			Console.WriteLine("");
		}

		public void DisconnectDevice()
		{
			DeviceClient.Close ();
		}

		public void DisconnectSimulator()
		{
			SimulatorClient.Disconnect ();
		}

		public void DisconnectDevices()
		{
			DisconnectDevice ();

			DisconnectSimulator ();
		}

		public void WaitForDevicesToConnect()
		{
			Thread.Sleep(DelayAfterConnectingToHardware);

			WaitForText(DataPrefix);

			ReadFromDeviceAndOutputToConsole();
		}

		public void HandleConnectionIOException(string deviceLabel, string devicePort, int deviceBaudRate, Exception exception)
		{
			if (exception.Message == "No such file or directory")
				throw new Exception("The " + deviceLabel + " device not found on port: " + devicePort + ". Please ensure it's connected via USB and that the port name is set correctly.", exception);
			else if (exception.Message == "Inappropriate ioctl for device")
				throw new Exception("The device serial baud rate appears to be incorrect: " + deviceBaudRate, exception);
			else
				throw exception;
		}
		#endregion

		#region Reset Functions
		public virtual void ResetDeviceViaPin()
		{
			// Close the connection to the device
			DisconnectDevice();

			// Set the reset trigger pin LOW (false) to begin a reset
			SimulatorClient.DigitalWrite (ResetTriggerPin, false);

			// Give the pin some time at LOW to ensure reset
			Thread.Sleep (10);

			// Change the reset trigger pin to an input to go back to normal
			// TODO: Implement a cleaner way to do this
			SimulatorClient.DigitalRead (ResetTriggerPin);

			// Re-open the connection to the device
			ConnectDevice ();

			// Ensure the irrigator restarted
			WaitForText (IrrigatorStartText);
		}
		#endregion

		#region Write to Device Functions
		public virtual void WriteToDevice(string text)
		{
			DeviceClient.WriteLine(text);
		}
		#endregion

		#region Read From Device Functions
		public string ReadLineFromDevice()
		{
			Console.WriteLine("Reading a line of the output from the device...");

			// Read the output
			var output = DeviceClient.ReadLine();

			FullDeviceOutput += output;

			ConsoleWriteSerialOutput(output);
			return output;
		}

		public void ReadFromDeviceAndOutputToConsole()
		{
			Console.WriteLine("");
			Console.WriteLine("Reading the output from the device...");
			Console.WriteLine("");

			// Read the output
			var output = DeviceClient.Read();

			FullDeviceOutput += output;

			ConsoleWriteSerialOutput(output);
			Console.WriteLine("");
		}
		#endregion

		#region Console Write Functions
		public void ConsoleWriteSerialOutput(string output)
		{
			if (!String.IsNullOrEmpty(output))
			{
				Console.WriteLine("----- Serial Output From Device");
				Console.WriteLine(output);
				Console.WriteLine("-------------------------------");
			}
		}
		#endregion

		#region Text Helper Functions
		public string GetOnOffString(bool onOffValue)
		{
			return (onOffValue ? "on" : "off");
		}
		#endregion

		#region Wait for Data Functions
		public Dictionary<string, string> WaitForDataEntry()
		{
			var dataString = WaitForDataLine();
			var dataEntry = ParseDataLine(dataString);
			return dataEntry;
		}

		public Dictionary<string, string>[] WaitForData(int numberOfEntries)
		{
			Console.WriteLine("");
			Console.WriteLine("Waiting for " + numberOfEntries + " data entries...");

			var list = new List<Dictionary<string, string>>();

			while (list.Count < numberOfEntries)
			{
				var dataEntry = WaitForDataEntry();
				list.Add(dataEntry);
				Console.WriteLine("");
			}

			return list.ToArray();
		}

		public string WaitForText(string text)
		{
			Console.WriteLine("Waiting for text: " + text);

			var output = String.Empty;
			var containsText = false;

			Timeout.Start();

			while (!containsText)
			{
				output += ReadLineFromDevice();

				if (output.Contains(text))
				{
					Console.WriteLine("  Found text: " + text);

					containsText = true;
				}
				else
					Timeout.Check(TimeoutWaitingForResponse, "Timed out waiting for text: " + text);
			}

			return output;
		}

		public string WaitForDataLine()
		{
			Console.WriteLine("Waiting for data line");

			var dataLine = String.Empty;
			var output = String.Empty;
			var containsData = false;

			var startTime = DateTime.Now;

			Timeout.Start();

			while (!containsData)
			{
				output += ReadLineFromDevice();

				var lastLine = GetLastLine(output);

				if (IsValidDataLine(lastLine))
				{
					Console.WriteLine("  Found valid data line");
					Console.WriteLine("    " + lastLine);

					containsData = true;
					dataLine = lastLine;
				}
				else
				{
					Timeout.Check(TimeoutWaitingForResponse, "Timed out waiting for data");
				}
			}

			return dataLine;
		}

		public double WaitUntilDataLine()
		{
			Console.WriteLine("Waiting for data line");

			var dataLine = String.Empty;
			var output = String.Empty;
			var containsData = false;

			var startTime = DateTime.Now;
			var timeInSeconds = 0.0;

			while (!containsData)
			{
				output += ReadLineFromDevice();

				var lastLine = GetLastLine(output);

				if (IsValidDataLine(lastLine))
				{
					Console.WriteLine("  Found valid data line");
					Console.WriteLine("    " + lastLine);

					containsData = true;
					dataLine = lastLine;
					timeInSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;
				}

				var hasTimedOut = DateTime.Now.Subtract(startTime).TotalSeconds > TimeoutWaitingForResponse;
				if (hasTimedOut && !containsData)
				{
					ConsoleWriteSerialOutput(output);

					Assert.Fail("Timed out waiting for data (" + TimeoutWaitingForResponse + " seconds)");
				}
			}

			return timeInSeconds;
		}

		public string GetLastLine(string output)
		{
			var lines = output.Trim().Split('\r');

			var lastLine = lines[lines.Length - 1];

			return lastLine;
		}
		#endregion

		#region Wait While/Until Simulator Pin Functions
		public int WaitUntilSimulatorPinIs(string label, int simulatorDigitalPin, bool expectedValue)
		{
			Console.WriteLine("Waiting until the " + label + " pin is " + GetOnOffString(expectedValue));

			bool powerPinValue = !expectedValue;

			var startTime = DateTime.Now;

			Timeout.Start();

			while (powerPinValue != expectedValue)
			{
				Timeout.Check(TimeoutWaitingForResponse, "Timed out waiting for simulator pin to switch to " + GetOnOffString(expectedValue));
				Console.Write(".");
				powerPinValue = SimulatorDigitalRead(simulatorDigitalPin);
			}
			Console.WriteLine("");

			var waitTimeInSeconds = DateTime.Now.Subtract(startTime).TotalSeconds;

			Console.WriteLine("  " + waitTimeInSeconds + " seconds");
			Console.WriteLine("");

			return (int)waitTimeInSeconds;
		}

		public double WaitWhileSimulatorPinIs(string label, int simulatorDigitalPin, bool expectedValue)
		{
			Console.WriteLine("Waiting while " + label + " pin is " + GetOnOffString(expectedValue));

			bool powerPinValue = !expectedValue;

			var startTime = DateTime.MinValue;
			var finishTime = DateTime.MinValue;

			bool isStarted = false;
			bool isFinished = false;

			while (!isFinished)
			{
				Console.Write(".");
				powerPinValue = SimulatorDigitalRead(simulatorDigitalPin);

				if (startTime == DateTime.MinValue && powerPinValue == expectedValue)
				{
					startTime = DateTime.Now;
					isStarted = true;
				}

				if (isStarted && powerPinValue != expectedValue)
				{
					finishTime = DateTime.Now;
					isFinished = true;
				}
			}
			Console.WriteLine("");

			var waitTimeInSeconds = finishTime.Subtract(startTime).TotalSeconds;

			Console.WriteLine("  " + waitTimeInSeconds + " seconds");
			Console.WriteLine("");

			return waitTimeInSeconds;
		}
		#endregion

		#region Simulator Read Pin Functions
		public bool SimulatorDigitalRead(int pinNumber)
		{
			return SimulatorClient.DigitalRead(pinNumber);
		}
		#endregion

		#region Data Value Assert Functions
		public void AssertDataValueEquals(Dictionary<string, string> dataEntry, string dataKey, int expectedValue)
		{
			var value = Convert.ToInt32(dataEntry[dataKey]);

			Assert.AreEqual(expectedValue, value, "Data value for '" + dataKey + "' key is incorrect: " + value);

			Console.WriteLine("Data value for '" + dataKey + "' is correct: " + value);
			Console.WriteLine("");
		}

		public void AssertDataValueIsWithinRange(Dictionary<string, string> dataEntry, string dataKey, int expectedValue, int allowableMarginOfError)
		{
			var value = Convert.ToInt32(dataEntry[dataKey]);

			var isWithinRange = IsWithinRange(expectedValue, value, allowableMarginOfError);

			var minValue = expectedValue - allowableMarginOfError;
			var maxValue = expectedValue + allowableMarginOfError;

			Assert.IsTrue(isWithinRange, "Data value for '" + dataKey + "' key is outside the specified range: " + value + " (Expected: " + minValue + " - " + maxValue + ")");

			Console.WriteLine("Data value for '" + dataKey + "' is within the valid range: " + value + " (Expected: " + minValue + " - " + maxValue + ")");
			Console.WriteLine("");
		}

		public void AssertIsWithinRange(string label, double expectedValue, double actualValue, double allowableMarginOfError)
		{
			var isWithinRange = IsWithinRange(expectedValue, actualValue, allowableMarginOfError);

			var minValue = expectedValue - allowableMarginOfError;
			var maxValue = expectedValue + allowableMarginOfError;

			Assert.IsTrue(isWithinRange, "The " + label + " value is outside the specified range: " + actualValue + " (Expected: " + minValue + " - " + maxValue + ")");

			Console.WriteLine("The " + label + " value is within the specified range: " + actualValue + " (Expected: " + minValue + " - " + maxValue + ")");
			Console.WriteLine("");
		}

		public bool IsWithinRange(double expectedValue, double actualValue, double allowableMarginOfError)
		{
			Console.WriteLine("Checking value is within range...");
			Console.WriteLine("  Expected value: " + expectedValue);
			Console.WriteLine("  Actual value: " + actualValue);
			Console.WriteLine("  Allowable margin of error: " + allowableMarginOfError);

			var minAllowableValue = expectedValue - allowableMarginOfError;
			if (minAllowableValue < 0)
				minAllowableValue = 0;
			var maxAllowableValue = expectedValue + allowableMarginOfError;

			Console.WriteLine("  Max allowable value: " + maxAllowableValue);
			Console.WriteLine("  Min allowable value: " + minAllowableValue);

			var isWithinRange = actualValue <= maxAllowableValue &&
				actualValue >= minAllowableValue;

			Console.WriteLine("Is within range: " + isWithinRange);

			return isWithinRange;
		}
		#endregion

		#region Simulator Pin Assert Functions
		public void AssertSimulatorPinForDuration(string label, int simulatorDigitalPin, bool expectedValue, int durationInSeconds)
		{
			Console.WriteLine("Checking soil " + label + " pin for specified duration...");
			Console.WriteLine("  Expected value: " + expectedValue);
			Console.WriteLine("  Duration: " + durationInSeconds);

			var startTime = DateTime.Now;

			var waitTimeIsFinished = false;

			while (!waitTimeIsFinished)
			{
				Console.Write(".");
				waitTimeIsFinished = DateTime.Now.Subtract(startTime).TotalSeconds > durationInSeconds;

				bool powerPinValue = SimulatorDigitalRead(simulatorDigitalPin);

				if (expectedValue)
					Assert.AreEqual(true, powerPinValue, "The " + label + " pin is off when it should be on.");
				else
					Assert.AreEqual(false, powerPinValue, "The " + label + " pin is on when it should be off.");
			}

			Console.WriteLine("");
			Console.WriteLine("The " + label + " pin works as expected.");
			Console.WriteLine("");
		}
		#endregion

		#region Data Parsing Functions
		public bool IsValidDataLine(string outputLine)
		{
			return outputLine.Trim().StartsWith(DataPrefix)
				&& outputLine.Trim().EndsWith(DataPostFix);
		}

		public Dictionary<string, string> ParseDataLine(string outputLine)
		{
			var dictionary = new Dictionary<string, string>();

			if (IsValidDataLine(outputLine))
			{
				foreach (var pair in outputLine.Split(';'))
				{
					var parts = pair.Split(':');

					if (parts.Length == 2)
					{
						var key = parts[0];
						var value = parts[1];
						dictionary[key] = value;
					}
				}
			}

			return dictionary;
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					ConsoleWriteSerialOutput(FullDeviceOutput);

					if (DeviceClient != null)
						DeviceClient.Close();

					if (SimulatorClient != null)
						SimulatorClient.Disconnect();

					Thread.Sleep(DelayAfterDisconnectingFromHardware);
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.

				disposedValue = true;
			}
		}

		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		// ~HardwareTestHelper() {
		//   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
		//   Dispose(false);
		// }

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			// GC.SuppressFinalize(this);
		}
		#endregion
	}
}
