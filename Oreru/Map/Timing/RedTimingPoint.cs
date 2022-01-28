using Oreru.Utils;

namespace Oreru.Map {
    public class RedTimingPoint : TimingPoint {
        public double Bpm {
            get => MillisecondsPerBeat / 1000 * 60;
            set => MillisecondsPerBeat = (int) (value / 60 * 1000);
        }
        
        public int MillisecondsPerBeat { get; set; }
        
        public RedTimingPoint(Timestamp timestamp, double bpm) : base(timestamp) { }
    }
}