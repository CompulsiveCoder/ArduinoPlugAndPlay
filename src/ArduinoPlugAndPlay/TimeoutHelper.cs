using System;

namespace ArduinoPlugAndPlay.Tests.Integration
{
    public class TimeoutHelper
    {
        public DateTime TimeoutStart = DateTime.MinValue;

        public TimeoutHelper ()
        {
        }

        #region Timeout Functions

        public void Start ()
        {
            TimeoutStart = DateTime.Now;
        }

        public void Check (int maxTime, string errorMessage)
        {
            var timeoutEnd = TimeoutStart.AddMilliseconds (maxTime);

            var hasTimedOut = timeoutEnd < DateTime.Now;

            if (hasTimedOut)
                throw new Exception (errorMessage);
        }

        #endregion

    }
}
