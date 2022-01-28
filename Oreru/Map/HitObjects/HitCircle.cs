using Oreru.Utils;

namespace Oreru.Map {
    public class HitCircle : HitObject {
        public HitCircle(Coordinate position, Timestamp time, bool newCombo = false) : base(position, time, newCombo) { }

        public override string ToString() => $"<HitCircle> {Position} @ {Timestamp}";
    }
}