using System;
using System.Configuration;

namespace ArduinoPlugAndPlay.ClientConsole
{
    public class ConfigHelper
    {
        public bool IsVerbose = false;

        public Arguments Arguments = new Arguments ();

        public ConfigHelper (Arguments arguments, bool isVerbose)
        {
            IsVerbose = isVerbose;
            Arguments = arguments;
        }

        public ConfigHelper (Arguments arguments)
        {
            Arguments = arguments;
        }

        public int GetInt32 (string argumentKey, int defaultValue)
        {
            if (String.IsNullOrWhiteSpace (argumentKey))
                return defaultValue;
            else if (!Arguments.Contains (argumentKey))
                return defaultValue;
            else {
                var value = 0;
                Int32.TryParse (GetValue (argumentKey), out value);
                if (value == 0)
                    value = defaultValue;
                return value;
            }
        }

        public string GetValue (string argumentKey)
        {
            var value = String.Empty;

            if (IsVerbose)
                Console.WriteLine ("Getting config/argument value for: " + argumentKey);

            if (Arguments.Contains (argumentKey)) {
                value = Arguments [argumentKey];
                if (IsVerbose)
                    Console.WriteLine ("Found in arguments");
            } else {

                try {
                    value = ConfigurationManager.AppSettings [argumentKey];
                } catch (Exception ex) {
                    Console.WriteLine ("Failed to get configuration value: " + argumentKey);
                    throw ex;
                }

                if (IsVerbose)
                    Console.WriteLine ("Looking in config");
            }

            return value;
        }

        public bool GetBoolean (string argumentKey)
        {
            var stringValue = GetValue (argumentKey);
            var boolValue = false;
            if (!String.IsNullOrEmpty (stringValue))
                Boolean.TryParse (stringValue, out boolValue);
            return boolValue;
        }

        public string[] GetArray (string argumentKey)
        {
            var stringValue = GetValue (argumentKey);

            return stringValue.Split (',');
        }
    }
}

