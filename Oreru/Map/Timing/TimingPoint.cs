using System;
using Oreru.Utils;

namespace Oreru.Map {
    public abstract class TimingPoint : IComparable<TimingPoint> {
        public Timestamp Timestamp { get; set; }

        private int _volume;
        public int Volume {
            get => _volume; 
            set {
                if (value >= 0 && value <= 100)
                    _volume = value;
            }
        }

        protected TimingPoint(Timestamp timestamp, int volume = 100) {
            Timestamp = timestamp;
            _volume = volume;
        }

        public int CompareTo(TimingPoint other) {
            return (Timestamp - other.Timestamp).Millis;
        }
    }
}