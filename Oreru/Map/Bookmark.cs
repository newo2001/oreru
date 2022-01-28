using System;
using Oreru.Utils;

namespace Oreru.Map {
    public class Bookmark : IComparable<Bookmark> {
        public Timestamp Timestamp { get; set; }

        public Bookmark(Timestamp timestamp) {
            Timestamp = timestamp;
        }

        public static bool operator <(Bookmark a, Bookmark b) {
            return a.Timestamp < b.Timestamp;
        }
        
        public static bool operator >(Bookmark a, Bookmark b) {
            return a.Timestamp > b.Timestamp;
        } 

        public int CompareTo(Bookmark other) {
            return (Timestamp - other.Timestamp).Millis;
        }

        public override string ToString() {
            return $"<Bookmark> {Timestamp}";
        }
    }
}