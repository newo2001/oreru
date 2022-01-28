using Oreru.Utils;

namespace Oreru.Map {
    public class Spinner : HitObject {
        public Timestamp EndTime { get; set; }
        
        // Actually a Duration not a timestamp..?
        public Timestamp Duration => EndTime - EndTime;

        public Spinner(Timestamp time, bool newCombo = false) : base((256, 192), time, newCombo) { }
    }
}