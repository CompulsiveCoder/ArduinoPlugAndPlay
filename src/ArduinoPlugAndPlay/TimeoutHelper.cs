using System;

namespace ArduinoPlugAndPlay
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

        public void Check (int maxMilliseconds, string errorMessage)
        {
            var timeoutEnd = TimeoutStart.AddMilliseconds (maxMilliseconds);

            var hasTimedOut = timeoutEnd < DateTime.Now;

            if (hasTimedOut)
                throw new Exception (errorMessage);
        }

        #endregion

    }
}
