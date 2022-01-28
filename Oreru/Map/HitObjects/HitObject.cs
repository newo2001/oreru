using System;
using Oreru.Utils;

namespace Oreru.Map {
    public abstract class HitObject : IComparable<HitObject> {

        private Coordinate _position;

        public HitSound HitSound { get; set; }

        public Coordinate Position {
            get => _position;
            set => MoveTo(value);
        }

        public Timestamp Timestamp { get; set; }

        public bool IsNewCombo { get; private set; }

        protected HitObject(Coordinate position, Timestamp time, bool newCombo = false) {
            Position = position;
            Timestamp = time;
            IsNewCombo = newCombo;
        }

        public void MoveTo(Coordinate pos) => _position = pos;
        
        // TODO: Figure out what the opposite of this is
        public void MarkNewCombo () => IsNewCombo = true;

        public int CompareTo(HitObject other) => (Timestamp - other.Timestamp).Millis;
    }
}