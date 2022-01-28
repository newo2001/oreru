using System;
using Microsoft.FSharp.Data.UnitSystems.SI.UnitNames;

namespace Oreru.Utils {
    public class Timestamp : IComparable<Timestamp> {
        public int Millis { get; set; }
        public int Seconds => Millis / 1000;
        public int Minutes => Seconds / 60;

        public Timestamp(int millis) {
            Millis = millis;
        }
        
        public static Timestamp FromTimecode(string timecode) {
            throw new NotImplementedException("fuck off");
        }

        public static implicit operator Timestamp(int millis) => new Timestamp(millis);

        public static Timestamp operator +(Timestamp a, Timestamp b) {
            return a.Millis + b.Millis;
        }

        public static Timestamp operator -(Timestamp a, Timestamp b) {
            return a.Millis - b.Millis;
        }

        public static bool operator <(Timestamp a, Timestamp b) {
            return a.Millis < b.Millis;
        }
        
        public static bool operator >(Timestamp a, Timestamp b) {
            return a.Millis > b.Millis;
        }

        public override string ToString() {
            var millis = Millis % 1000;
            var seconds = Seconds % 60;
            var minutes = Minutes / 60;
            
            return $"{minutes}:{seconds:00}:{millis:000}";
        }

        public int CompareTo(Timestamp other) => Millis - other.Millis;
    }
}