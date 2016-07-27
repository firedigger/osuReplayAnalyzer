using System;

namespace ReplayAPI
{
    public class ReplayFrame
    {
        public int TimeDiff;
        public int Time;
        [System.ComponentModel.DisplayName("Time In Seconds")]
        public float TimeInSeconds { get { return Time / 1000f; } }
        public float X { get; set; }
        public float Y { get; set; }
        public Keys Keys { get; set; }
        public KeyCounter keyCounter { get; set; }
        public int combo { get; set; }
        public double travelledDistance { get; set; }
        public double speed { get; set; }
        public double acceleration { get; set; }
    }
}
